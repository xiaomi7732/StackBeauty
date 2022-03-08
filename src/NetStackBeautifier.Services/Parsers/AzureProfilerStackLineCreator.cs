using System.Text.RegularExpressions;
using NetStackBeautifier.Core;

namespace NetStackBeautifier.Services.StackBeautifiers;

internal class AzureProfilerStackLineCreator
{
    // Matches group1!group2, the first `!` is the separator.
    private const string PartsMatcherExp = @"(.*?)!(.*)";
    private static readonly Regex _partsMatcher = new Regex(PartsMatcherExp, RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));

    public IFrameLine Create(FrameClassNameFactory classNameFactory, string line)
    {
        Match match = _partsMatcher.Match(line);

        if(!match.Success)
        {
            throw new InvalidCastException($"Can't understand the text. There should have at least 1 exclaim sign (!). Regex: {PartsMatcherExp}, Content: {line}");
        }

        FrameFullClass fullClass = classNameFactory.FromString(match.Groups[1].Value);
        FrameMethod method = new FrameMethod() { Name = match.Groups[2].Value };

        return new FrameItem()
        {
            FullClass = fullClass,
            Method = method,
        };
    }
}