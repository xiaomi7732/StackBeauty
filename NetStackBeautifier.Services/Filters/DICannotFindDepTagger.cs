using System.Text.RegularExpressions;
using NetStackBeautifier.Core;
using NetStackBeautifier.Services.StackBeautifiers;

namespace NetStackBeautifier.Services.Filters;

/// <summary>
/// Tags a text frame that matches the specific exception: Unable to resolve service for type 'NetStackBeautifier.Core.FrameClassNameFactory' while attempting to activate 'NetStackBeautifier.Services.SimpleExceptionStackBeautifier'.
/// </summary>
internal class DICannotFindDepTagger : IFrameFilter<SimpleExceptionStackBeautifier>
{
    // Match the target.
    // Group[1]-Missing Service-NetStackBeautifier.Core.FrameClassNameFactory
    // Group[2]-Creating instance-NetStackBeautifier.Services.SimpleExceptionStackBeautifier
    private const string MatchExp = @"Unable to resolve service for type '(.*?)' while attempting to activate '(.*?)'\.";
    private static readonly Regex _matcher = new Regex(MatchExp, RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));

    public void Filter(IFrameLine frameline)
    {
        if (frameline is not FrameRawText rawText)
        {
            return;
        }

        Match match = _matcher.Match(rawText.Value);
        if (!match.Success)
        {
            return;
        }

        string missingType = match.Groups[1].Value.Split('.', StringSplitOptions.RemoveEmptyEntries).Last();
        string targetType = match.Groups[2].Value.Split('.', StringSplitOptions.RemoveEmptyEntries).Last();

        bool isMissingInterface = missingType.StartsWith("I");

        string markdown = $"It looks like the registered type of `{targetType}` is **missing dependency of** `{missingType}`. Is it possible, type of `{missingType}` is not registered in the DI container?";
        if (isMissingInterface)
        {
            markdown += $" The missing type of `{missingType}` does look like an interface. Maybe, the class is regstered, but not the interface?";
        }
        else
        {
            markdown += $" The missing type of `{missingType}` looks like a class, do you intend to inject an interface of it in the constructor of `{targetType}` instead?";
        }
        frameline.Tags.TryAdd("AnalysisMarkDown", markdown);
    }
}