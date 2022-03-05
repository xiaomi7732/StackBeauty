using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using NetStackBeautifier.Core;

namespace NetStackBeautifier.Services.LineBeautifiers;

internal class ClassNameTickNumberRemover<T> : LineBeautifierBase<T>
    where T: BeautifierBase<T>
{
    private const string matcherExpression = @"^.*(`\d+).*$";
    private static readonly Regex matcher = new Regex(matcherExpression, RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));

    public ClassNameTickNumberRemover(ILogger<ClassNameTickNumberRemover<T>> logger) : base(logger)
    {
    }

    protected override Task<FrameItem> BeautifyImpAsync(FrameItem line, CancellationToken cancellationToken = default)
    {
        string? shortClassName = line.FullClass?.ShortClassNameOrDefault;
        if (string.IsNullOrEmpty(shortClassName) || shortClassName.IndexOf('`', StringComparison.Ordinal) == -1)
        {
            return Task.FromResult(line);
        }

        if (line.FullClass is null)
        {
            return Task.FromResult(line);
        }

        string processedClassName = matcher.Replace(shortClassName, (match) => match.Groups[0].Value.Replace(match.Groups[1].Value, ""));
        int remainSectionCount = line.FullClass.NameSections.Count() - 1;
        line = line with
        {
            FullClass = line.FullClass! with
            {
                NameSections = line.FullClass.NameSections.Take(remainSectionCount).Append(processedClassName),
            },
        };

        return Task.FromResult(line);
    }
}