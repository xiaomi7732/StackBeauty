using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NetStackBeautifier.Core;
using NetStackBeautifier.Services.LineBeautifiers;
using NetStackBeautifier.Services.StackBeautifiers;
using Xunit;

namespace NetStackBeautifier.Services.Tests;

public class DerivedClassBeautifierTests
{
    [Fact]
    public async Task ShouldProcessDerivedClassBaseClassInClassSections()
    {
        string input = @"NetStackBeautifier.Services.dll!NetStackBeautifier.Services.BeautifierBase<Services.StackBeautifiers.SimpleExceptionStackBeautifier>.<BeautifyAsync>d__5.MoveNext()";
        IEnumerable expectedClassSections = new List<string>(){
            "Services", "StackBeautifiers", "(SimpleExceptionStackBeautifier)BeautifierBase", "<BeautifyAsync>d__5", "MoveNext()"
        };

        DerivedClassBeautifier<AIProfilerStackBeautifier> target = new DerivedClassBeautifier<AIProfilerStackBeautifier>(FrameClassNameFactory.Instance, new Mock<ILogger<DerivedClassBeautifier<AIProfilerStackBeautifier>>>().Object);
        FrameItem testInput = new FrameItem()
        {
            FullClass = FrameClassNameFactory.Instance.FromString(input),
        };

        FrameItem actual = await target.BeautifyAsync(testInput, default).ConfigureAwait(false);

        Assert.NotNull(actual);
        Assert.NotNull(actual?.FullClass);

        Assert.Equal(expectedClassSections, actual!.FullClass!.NameSections);
    }
}