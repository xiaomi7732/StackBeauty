using System.Text.RegularExpressions;
using NetStackBeautifier.Core;

namespace NetStackBeautifier.Services.StackBeautifiers;

/// <summary>
/// Match line info like this:  in D:\Demo\LearnThrow\Program.cs:line 49
/// Starts with `in `, followed by file and line number.
/// </summary>
internal class PlainExceptionLineInfoMatcher : RegexLineInfoMatcherBase
{
    // Matches:  in D:\Demo\LearnThrow\Program.cs:line 49
    // Group 1: File Path ==> D:\Demo\LearnThrow\Program.cs
    // Group 2: Line number ==> 49
    private const string MatchExp = @"^\s*in\s+(.*):.*\s+([\d]+)$";
    public PlainExceptionLineInfoMatcher() : base(MatchExp)
    {
    }

    protected override FrameFileInfo CreateOnSuccessMatch(Match match) =>
        new FrameFileInfo(
            FilePath: match.Groups[1].Success ? match.Groups[1].Value : string.Empty,
            LineNumber: match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : 0
        );
}