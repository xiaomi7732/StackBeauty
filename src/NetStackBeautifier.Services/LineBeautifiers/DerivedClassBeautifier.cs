using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using NetStackBeautifier.Core;

namespace NetStackBeautifier.Services.LineBeautifiers;

/// <summary>
/// Unwraps derived class.
/// For example:
/// NetStackBeautifier.Services.BeautifierBase<NetStackBeautifier.Services.StackBeautifiers.SimpleExceptionStackBeautifier> means
/// NetStackBeautifier.Services.StackBeautifiers.SimpleExceptionStackBeautifier inherits NetStackBeautifier.Services.BeautifierBase.
/// And should be displayed as: SimpleExceptionStackBeautifier : BeautifierBase
/// </summary>
internal class DerivedClassBeautifier<T> : LineBeautifierBase<T>
    where T : IBeautifier
{
    private const string InheritClassMatcher = @"^([\w]+?)<(.*)>$";
    private static readonly Regex _inheritClassMatcher = new Regex(InheritClassMatcher, RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));
    private readonly FrameClassNameFactory _frameClassNameFactory;

    public DerivedClassBeautifier(
        FrameClassNameFactory frameClassNameFactory,
        ILogger<LineBeautifierBase<T>> logger) : base(logger)
    {
        _frameClassNameFactory = frameClassNameFactory ?? throw new ArgumentNullException(nameof(frameClassNameFactory));
    }

    protected override Task<FrameItem> BeautifyImpAsync(FrameItem line, CancellationToken cancellationToken = default)
    {
        if (line.FullClass is null)
        {
            return Task.FromResult(line);
        }

        string[] classSections = line.FullClass.NameSections.ToArray();

        Match? match = null;
        int matchIndex = -1;
        for (int i = 0; i < classSections.Length; i++)
        {
            match = _inheritClassMatcher.Match(classSections[i]);
            if (match.Success)
            {
                matchIndex = i;
                break;
            }
        }

        if (match is null || !match.Success)
        {
            return Task.FromResult(line);
        }

        FrameFullClass newFullClass = _frameClassNameFactory.FromString(match.Groups[2].Value);
        IEnumerable<string> newSections = newFullClass.NameSections;
        if (match.Groups[1].Success && !string.IsNullOrEmpty(match.Groups[1].Value))
        {
            string last = newSections.Last() + "(:" + match.Groups[1].Value + ")";
            newSections = newSections.SkipLast(1).Append(last);
        }

        return Task.FromResult(line with
        {
            FullClass = line.FullClass with
            {
                NameSections = newSections.Union(classSections.Skip(matchIndex + 1)),
            },
        });
    }

}