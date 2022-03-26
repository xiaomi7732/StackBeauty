using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using NetStackBeautifier.Core;

namespace NetStackBeautifier.Services.LineBeautifiers;

internal class MoveNextBeautifier<T> : LineBeautifierBase<T>
    where T : IBeautifier
{
    // Matches state machine info.
    // For example: aa<bbaera>M<>anagedIdentitySource+<AuthenticateAsync>d__9 will match
    // Group[1]-ClassName-aa<bbaera>M<>anagedIdentitySource
    // Group[2]-MethodName-AuthenticateAsync
    // Optionally, the method might be a generics, for example: CacheExtensions+<GetOrCreateAsync>d__9`1
    // Group[1]-ClassName-CacheExtensions
    // Group[2]-MethodName-GetOrCreateAsync
    // Group[3]-Generic Parameter Count -1
    private const string StateMachineMethodMatherExp = @"^(.*)\+<(.*?)>d[_]*[\d]*(?:`([\d])+)*$";
    private static readonly Regex _stateMachineMethodMatcher = new Regex(StateMachineMethodMatherExp, RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));

    // For example: <BeautifyAsync>d__5
    // Group [1]: Method Name: 
    private const string StateMachineMethodMatherExp2 = @"^<(.*)>d__[\d]*$";
    private static readonly Regex _stateMachineMethodMatcher2 = new Regex(StateMachineMethodMatherExp2, RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));

    // Match DisplayClass tag in class
    // For example: CentralStorageAuthority+<>c__DisplayClass2_1
    // Group[1]-ClassName-CentralStorageAuthority
    // Group[2]-Display class place holder-+<>c__DisplayClass2_1
    private const string DisplayClassCleanerExp = @"(.*)\+<.*?c__DisplayClass[\d]+_[\d]+$";
    private static readonly Regex _displayClassHolderMatcher = new Regex(DisplayClassCleanerExp, RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));

    // Lambda expression in this form: <GetCacheSettingsAsync>b__0
    // Group[1]-Lambda method-GetCacheSettingsAsync
    private const string DeLambdaExp = @"^<(.*?)>b(?:__[\d]*)?$";
    private static readonly Regex _deLambdaMatcher = new Regex(DeLambdaExp, RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));
    public MoveNextBeautifier(ILogger<MoveNextBeautifier<T>> logger) : base(logger)
    {
    }

    protected override Task<FrameItem> BeautifyImpAsync(FrameItem line, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(line.Method?.Name) || string.IsNullOrEmpty(line.FullClass?.ShortClassNameOrDefault))
        {
            return Task.FromResult(line);
        }

        if (string.Equals(line.Method.Name, "MoveNext", StringComparison.OrdinalIgnoreCase))
        {
            Match match = _stateMachineMethodMatcher.Match(line.FullClass.ShortClassNameOrDefault);
            if (match.Success)
            {
                string newShortClassName = CleanUpDisplayClass(match.Groups[1].Value);
                string newMethodName = CleanupMethod(match.Groups[2].Value);

                int genericMethodCount = 0;
                if (match.Groups.Count >= 4 && int.TryParse(match.Groups[3].Value, out genericMethodCount))
                {
                    // genericMethodCount has been assigned upon success already.
                }

                line = line with
                {
                    FullClass = line.FullClass with
                    {
                        NameSections = line.FullClass.NameSections.SkipLast(0).Append(newShortClassName),
                    },
                    Method = line.Method with
                    {
                        Name = newMethodName,
                        GenericParameterTypes = line.Method.GenericParameterTypes.NullAsEmpty().Any() ? line.Method.GenericParameterTypes : GenerateTypeParameter(genericMethodCount),
                    },
                };

                return Task.FromResult(line);
            }

            match = _stateMachineMethodMatcher2.Match(line.FullClass.ShortClassNameOrDefault);
            if (match.Success)
            {
                string newMethodName = match.Groups[1].Value;

                return Task.FromResult(line with
                {
                    FullClass = line.FullClass with
                    {
                        NameSections = line.FullClass.NameSections.SkipLast(1),
                    },
                    Method = line.Method with
                    {
                        Name = newMethodName,
                    }
                });
            }
        }
        return Task.FromResult(line);
    }

    private IEnumerable<string> GenerateTypeParameter(int count)
    {
        if (count <= 0)
        {
            return Enumerable.Empty<string>();
        }

        List<string> results = new List<string>();
        for (int i = 0; i < count; i++)
        {
            if (i == 0)
            {
                results.Add("T");
                continue;
            }
            int suffix = i + 1;
            results.Add("T" + suffix);
        }
        return results;
    }

    private string CleanUpDisplayClass(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        Match match = _displayClassHolderMatcher.Match(input);
        if (!match.Success)
        {
            return input;
        }

        return match.Groups[1].Value;
    }

    private string CleanupMethod(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        Match match = _deLambdaMatcher.Match(input);
        if (!match.Success)
        {
            return input;
        }

        return "() => " + match.Groups[1].Value;
    }
}