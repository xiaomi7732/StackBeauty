using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NetStackBeautifier.Core;
using NetStackBeautifier.Services.Filters;
using NetStackBeautifier.Services.StackBeautifiers;
using Xunit;
using Xunit.Sdk;

namespace NetStackBeautifier.Services.Tests;

/// <summary>
/// Special case tests when no symbol text shows in the callstack
/// </summary>
public class AzureProfilerStackBeautifierNoSymbolTests
{
    // There is a filter rely on starting with this prefix. Change it with caution.
    private const string _rawTextPrefix = @"Missing symbol in assembly [";

    /// <summary>
    /// Test that symbol missing info would be treat as noise for the end user.
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task NoSymbolIsDetectedAsync()
    {
        string input = @"mscorlib.ni.dll!6270759 [mscorlib.ni.pdb/fa1a429d998490881c873b0a3d3cc3bf1]";
        AzureProfilerStackBeautifier target = new AzureProfilerStackBeautifier(
            new LineBreaker(),
            Enumerable.Empty<ILineBeautifier<AzureProfilerStackBeautifier>>(),
            Enumerable.Empty<IFrameFilter<AzureProfilerStackBeautifier>>(),
            Enumerable.Empty<IPreFilter<AzureProfilerStackBeautifier>>(),
            FrameClassNameFactory.Instance,
            new AzureProfilerStackLineCreator()
            );


        Assert.True(target.CanBeautify(input), "Should take the input.");

        IFrameLine? carryOn = null;
        await foreach (IFrameLine onlyLine in target.BeautifyAsync(input, default))
        {
            if (onlyLine is not FrameRawText rawText)
            {
                throw new XunitException("Should only be 1 line of raw text. Anything changed?");
            }

            Assert.Equal(_rawTextPrefix + "mscorlib.ni.dll]. Symbol signature: [mscorlib.ni.pdb/fa1a429d998490881c873b0a3d3cc3bf1].", rawText.Value);
            carryOn = onlyLine;
        }

        Assert.NotNull(carryOn);

        NoiseFilter<AzureProfilerStackBeautifier> noiseFilter = new NoiseFilter<AzureProfilerStackBeautifier>();
        noiseFilter.Filter(carryOn!);

        Assert.Equal(new Dictionary<string, string>()
        {
            [KnownTagKeys.Noise] = "true"
        }, carryOn!.Tags);
    }
}