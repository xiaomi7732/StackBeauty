using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using NetStackBeautifier.Core;

namespace NetStackBeautifier.Services.LineBeautifiers;

internal class LambdaMethodBeautifier<T> : LineBeautifierBase<T>
    where T : IBeautifier
{

    // Expression that matches a lambda method. For example:
    // <EnumerateLevelAsync>b__1
    // Group[1]:MethodName:EnumerateLevelAsync
    private const string LambdaMethodMatchExp = @"^\<(.*)?\>b__(?:\d)*$";
    private static readonly Regex _lambdaMethodMatcher = new Regex(LambdaMethodMatchExp, RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));

    // Matches DisplayClass for example:
    // Microsoft.Commerce.HierarchyParticipantExtensions+<>c__DisplayClass3_2`2[System.__Canon,System.__Canon]
    // Matches `+<>c__DisplayClass3_2`
    private const string DisplayClassRemover = @"\+\<\>c__DisplayClass[\d]*_[\d]*";
    private static readonly Regex _displayClassRemover = new Regex(DisplayClassRemover, RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));

    public LambdaMethodBeautifier(ILogger<LineBeautifierBase<T>> logger) : base(logger)
    {
    }

    protected override Task<FrameItem> BeautifyImpAsync(FrameItem line, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(line.Method?.Name))
        {
            return Task.FromResult(line);
        }

        // Method
        Match methodMatch = _lambdaMethodMatcher.Match(line.Method.Name);
        if (!methodMatch.Success)
        {
            return Task.FromResult(line);
        }

        string lambdaMethodName = methodMatch.Groups[1].Value;

        // Optionally Update class
        IEnumerable<string> newClassNameSections = Enumerable.Empty<string>();
        if (!string.IsNullOrEmpty(line.FullClass?.ShortClassNameOrDefault) && _displayClassRemover.Match(line.FullClass.ShortClassNameOrDefault).Success)
        {
            string shortClassName = _displayClassRemover.Replace(line.FullClass.ShortClassNameOrDefault, string.Empty);
            newClassNameSections = line.FullClass.NameSections.SkipLast(1).Append(shortClassName);
        }

        return Task.FromResult(line with
        {
            FullClass = line.FullClass with
            {
                NameSections = newClassNameSections.Any() ? newClassNameSections : line.FullClass.NameSections,
            },
            Method = line.Method with
            {
                Name = $"() => {{.. {lambdaMethodName}(); ..}}"
            }
        });
    }
}