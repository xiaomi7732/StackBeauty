using System.Text.RegularExpressions;
using NetStackBeautifier.Core;

namespace NetStackBeautifier.Services.StackBeautifiers;

/// <summary>
/// Matches line info in parentheses, example: (d:\Repos\NetStackBeautifier\src\NetStackBeautifier.Services\Parsers\SimpleExceptionStackBeautifier.cs:59)
/// </summary>
internal class PathColonLineNumberMatcher : RegexLineInfoMatcherBase
{
    // Matches line info in parentheses.
    // For example: (d:\Repos\NetStackBeautifier\src\NetStackBeautifier.Services\Parsers\SimpleExceptionStackBeautifier.cs:59)
    // Group 1: FilePath  ==> d:\Repos\NetStackBeautifier\src\NetStackBeautifier.Services\Parsers\SimpleExceptionStackBeautifier.cs
    // Group 2: Line Number  ==> 59
    private const string Regex = @"\((.*):\s*([\d]+)\)";
    public PathColonLineNumberMatcher() : base(Regex)
    {
    }

    protected override FrameFileInfo CreateOnSuccessMatch(Match match)
    {
        return new FrameFileInfo(
            FilePath: match.Groups[1].Value,
            LineNumber: match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : 0
        );
    }
}