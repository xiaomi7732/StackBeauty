using NetStackBeautifier.Core;
using NetStackBeautifier.Services.StackBeautifiers;
using Xunit;

namespace NetStackBeautifier.Services.Tests;

public class AzureProfilerStackLineCreatorTests
{
    [Fact]
    public void ShouldHandleMultipleExclaimSigns()
    {
        string input = @"System.Collections.Generic.List`1[System.Int64]!ForEach(class System.Action`1<!0>)";
        AzureProfilerStackLineCreator target  = new AzureProfilerStackLineCreator();
        FrameItem? result = target.Create(Core.FrameClassNameFactory.Instance, input) as FrameItem;

        Assert.NotNull(result);
        Assert.NotNull(result!.FullClass);
        Assert.NotNull(result!.Method);

        Assert.Equal("System.Collections.Generic.List`1", string.Join(".", result!.FullClass!.NameSections.NullAsEmpty()));
        Assert.Equal("ForEach(class System.Action`1<!0>)", result.Method!.Name);
    }
}