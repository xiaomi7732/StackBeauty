using System.Collections.Generic;
using System.Linq;
using NetStackBeautifier.Core;
using Xunit;

namespace NetStackBeautifier.Services.Tests;

public class FrameClassNameFactoryTests
{
    [Fact]
    public void ShouldParseSectionNames()
    {
        string line = @"Microsoft.Commerce.Collections.KeyedCollection`2[System.__Canon,System.__Canon]";

        FrameFullClass fullNames = FrameClassNameFactory.Instance.FromString(line);

        Assert.Equal(new List<string>() { "Microsoft", "Commerce", "Collections", "KeyedCollection`2" }, fullNames.NameSections);

        Assert.Equal(new List<string>() { "System.__Canon", "System.__Canon" }, fullNames.GenericParameterTypes);
    }

    [Fact]
    public void ShouldParseSimpleSectionNames()
    {
        string line = @"NetStackBeautifier.Services.Tests.ShouldParseSimpleSectionNames";

        FrameFullClass fullNames = FrameClassNameFactory.Instance.FromString(line);

        Assert.Equal(new List<string>() { "NetStackBeautifier", "Services", "Tests", "ShouldParseSimpleSectionNames" }, fullNames.NameSections);

        Assert.Equal(Enumerable.Empty<string>(), fullNames.GenericParameterTypes);
    }
}