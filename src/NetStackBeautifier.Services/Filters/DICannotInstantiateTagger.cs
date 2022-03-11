using System.Text.RegularExpressions;
using NetStackBeautifier.Core;
using NetStackBeautifier.Services.StackBeautifiers;

namespace NetStackBeautifier.Services.Filters;

internal class DICannotInstantiateTagger : IFrameFilter<SimpleExceptionStackBeautifier>
{
    // Matches Cannot instantiate implementation type 'NetStackBeautifier.Services.BeautifierService' for service type 'NetStackBeautifier.Services.IBeautifierService'.
    private const string MatchExp = @"Cannot instantiate implementation type '(.*?)' for service type '(.*?)'";
    private static Regex _match = new Regex(MatchExp, RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));

    public void Filter(IFrameLine frameline)
    {
        if (frameline is not FrameRawText rawText)
        {
            return;
        }

        Match match = _match.Match(rawText.Value);
        if (!match.Success)
        {
            return;
        }

        string implementType = match.Groups[1].Value.Split('.', StringSplitOptions.RemoveEmptyEntries).Last();
        string serviceType = match.Groups[2].Value.Split('.', StringSplitOptions.RemoveEmptyEntries).Last();
        bool impTypeInterface = implementType.StartsWith('I');

        string markdown = $"It looks like the DI container is trying to get an instance for `{serviceType}`. However, it cannot create an object of `{implementType}`.";
        if (impTypeInterface)
        {
            markdown += $" `{implementType}` looks like **an interface**. Is it registered as **an implementation type** by accident?";
        }
        else
        {
            markdown += $" by name, `{implementType}` does look like a class, is it possible it is an **abstract class** or, by chance, is it **an interface**?";
        }

        frameline.Tags.TryAdd(KnownTagKeys.AnalysisMarkDown, markdown);
    }
}