using System.Text.RegularExpressions;
using NetStackBeautifier.Core;

namespace NetStackBeautifier.Services.StackBeautifiers;

/// <summary>
/// Base class for line info regex matcher
/// </summary>
internal abstract class RegexLineInfoMatcherBase : ILineInfoMatcher
{
    private readonly string _matchExp;
    private readonly Regex _matcher;

    public RegexLineInfoMatcherBase(string regexExpression)
    {
        if (string.IsNullOrWhiteSpace(regexExpression))
        {
            throw new ArgumentException($"'{nameof(regexExpression)}' cannot be null or whitespace.", nameof(regexExpression));
        }

        _matchExp = regexExpression;
        _matcher = new Regex(_matchExp, RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));
    }

    public bool TryCreate(string input, out FrameFileInfo? fileInfo)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new ArgumentException($"'{nameof(input)}' cannot be null or whitespace.", nameof(input));
        }

        Match match = _matcher.Match(input);
        if (!match.Success)
        {
            fileInfo = null;
            return false;
        }

        fileInfo = CreateOnSuccessMatch(match);
        return true;
    }

    protected abstract FrameFileInfo CreateOnSuccessMatch(Match match);
}