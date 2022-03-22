using System.Text.RegularExpressions;
using NetStackBeautifier.Core;

namespace NetStackBeautifier.Services.StackBeautifiers;

/// <summary>
/// Matches line info like this: Line 42 C#
/// </summary>
internal class LineNumberLangMatcher : RegexLineInfoMatcherBase
{
    // Matches anything starts with 'Line' like:  Line 42 C#
    // Group 1: Line number ==> 42
    private const string MatchExp = @"^\s*Line ([\d]+)";

    public LineNumberLangMatcher() : base(MatchExp)
    {
    }

    protected override FrameFileInfo CreateOnSuccessMatch(Match match)
    {
        return new FrameFileInfo(string.Empty, int.Parse(match.Groups[1].Value));
    }
}