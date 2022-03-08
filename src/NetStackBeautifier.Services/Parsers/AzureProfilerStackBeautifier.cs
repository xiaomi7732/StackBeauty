using System.Text.RegularExpressions;
using NetStackBeautifier.Core;
using NetStackBeautifier.Services.Filters;

namespace NetStackBeautifier.Services.StackBeautifiers;

/// <summary>
/// This beautifier handles Azure Profiler Trace stack
/// </summary>
internal class AzureProfilerStackBeautifier : BeautifierBase<AzureProfilerStackBeautifier>
{
    // Missing symbol frame example: mscorlib.ni.dll!6270759 [mscorlib.ni.pdb/fa1a429d998490881c873b0a3d3cc3bf1]
    // Group[1]: Assembly name: mscorlib.ni.dll
    // Group[2]: Symbol signature: mscorlib.ni.pdb/fa1a429d998490881c873b0a3d3cc3bf1
    private const string MissingSymbolMatchExpression = @"^(.*)![\d]*\s\[(.*)\]$";
    private static readonly Regex _missingSymbolMatcher = new Regex(MissingSymbolMatchExpression, RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));

    private readonly LineBreaker _lineBreaker;
    private readonly FrameClassNameFactory _frameClassNameFactory;
    private readonly AzureProfilerStackLineCreator _lineCreator;

    public AzureProfilerStackBeautifier(
        LineBreaker lineBreaker,
        IEnumerable<ILineBeautifier<AzureProfilerStackBeautifier>> lineBeautifiers,
        IEnumerable<IFrameFilter<AzureProfilerStackBeautifier>> filters,
        IEnumerable<IPreFilter<AzureProfilerStackBeautifier>> preFilters,
        FrameClassNameFactory frameClassNameFactory,
        AzureProfilerStackLineCreator lineCreator
        ) : base(lineBreaker, lineBeautifiers, filters, preFilters)
    {
        _lineBreaker = lineBreaker ?? throw new ArgumentNullException(nameof(lineBreaker));
        _frameClassNameFactory = frameClassNameFactory ?? throw new ArgumentNullException(nameof(frameClassNameFactory));
        _lineCreator = lineCreator ?? throw new ArgumentNullException(nameof(lineCreator));
    }

    public override bool CanBeautify(string input)
    {
        foreach (string line in _lineBreaker.BreakIntoLines(input))
        {
            if (!line.Contains('!', StringComparison.Ordinal))
            {
                return false;
            }
        }
        return true;
    }

    protected override IFrameLine CreateFrameItem(string line)
    {
        if (_missingSymbolMatcher.IsMatch(line))
        {
            return CreateMissingSymbolLine(line);
        }

        return CreateNormalLine(line);
    }

    private IFrameLine CreateMissingSymbolLine(string line)
    {
        Match match = _missingSymbolMatcher.Match(line);
        if (!match.Success)
        {
            throw new InvalidCastException("Unexpected match failure. How could this happen?");
        }

        if (match.Groups.Count != 3)
        {
            throw new InvalidCastException("Unexpected matching groups for missing symbol line.");
        }

        return new FrameRawText()
        {
            Value = $"Missing symbol in assembly [{match.Groups[1]}]. Symbol signature: [{match.Groups[2]}].",
        };
    }

    private IFrameLine CreateNormalLine(string line) => _lineCreator.Create(_frameClassNameFactory, line);
}