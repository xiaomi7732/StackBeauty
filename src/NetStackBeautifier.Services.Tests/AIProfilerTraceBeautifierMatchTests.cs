using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NetStackBeautifier.Core;
using NetStackBeautifier.Services.Filters;
using NetStackBeautifier.Services.StackBeautifiers;
using Xunit;

namespace NetStackBeautifier.Services.Tests;

public class AIProfilerTraceBeautifierMatchTests
{
    [Fact]
    public async Task ShouldAcceptInputsAsync()
    {
        string inputFilePattern = "AITraceInput*.txt";

        AIProfilerStackBeautifier beautifier = new AIProfilerStackBeautifier(
            new LineBreaker(),
            Enumerable.Empty<ILineBeautifier<AIProfilerStackBeautifier>>(),
            Enumerable.Empty<IFrameFilter<AIProfilerStackBeautifier>>(),
            Enumerable.Empty<IPreFilter<AIProfilerStackBeautifier>>(),
            new AIProfilerStackManagedSignatureMatcher(),
            FrameClassNameFactory.Instance);

        foreach (string testFilePath in Directory.EnumerateFiles(".", inputFilePattern))
        {
            using (Stream input = File.OpenRead(testFilePath))
            using (StreamReader reader = new StreamReader(input))
            {
                string inputStack = await reader.ReadToEndAsync().ConfigureAwait(false);
                bool actual = beautifier.CanBeautify(inputStack);
                Assert.True(actual, $"Should be able to beautify. Input Filename: {testFilePath}");
            }
        }
    }
}