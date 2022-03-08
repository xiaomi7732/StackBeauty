using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NetStackBeautifier.Core;
using NetStackBeautifier.Services.LineBeautifiers;
using NetStackBeautifier.Services.StackBeautifiers;
using Xunit;

namespace NetStackBeautifier.Services.Tests;

public class LambdaMethodBeautifierTest
{
    /// <summary>
    /// When the Method is `<EnumerateLevelAsync>b__1`, and class name is `Microsoft.Commerce.HierarchyParticipantExtensions+<>c__DisplayClass3_2`2[System.__Canon,System.__Canon]`,
    /// Method becomes: ()=> {.. EnumerateLevelAsync(); .. }
    /// Class becomes: Microsoft.Commerce.HierarchyParticipantExtensions`2[System.__Canon,System.__Canon]
    /// </summary>
    /// <param name="input"></param>
    /// <param name="expected"></param>
    [Fact]
    public async Task BeautifyAsExpectedAsync()
    {
        FrameItem input = new FrameItem()
        {
            FullClass = FrameClassNameFactory.Instance.FromString("Microsoft.Commerce.HierarchyParticipantExtensions+<>c__DisplayClass3_2`2[System.__Canon,System.__Canon]"),
            Method = new FrameMethod() { Name = "<EnumerateLevelAsync>b__1" },
        };

        Mock<ILogger<LambdaMethodBeautifier<AIProfilerStackBeautifier>>> loggerMock = new Mock<ILogger<LambdaMethodBeautifier<AIProfilerStackBeautifier>>>();

        LambdaMethodBeautifier<AIProfilerStackBeautifier> target = new LambdaMethodBeautifier<AIProfilerStackBeautifier>(loggerMock.Object);
        FrameItem output = await target.BeautifyAsync(input, default).ConfigureAwait(false);


        Assert.Equal("() => {.. EnumerateLevelAsync(); ..}", output.Method?.Name);
        Assert.Equal(@"HierarchyParticipantExtensions`2", output.FullClass?.ShortClassNameOrDefault);
    }
}