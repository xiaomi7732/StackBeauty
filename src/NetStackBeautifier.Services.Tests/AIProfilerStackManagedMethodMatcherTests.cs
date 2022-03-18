using System.Linq;
using NetStackBeautifier.Core;
using NetStackBeautifier.Services.StackBeautifiers;
using Xunit;

namespace NetStackBeautifier.Services.Tests;

public class AIProfilerStackManagedMethodMatcherTests
{
    [Theory]
    [InlineData(@"System.Web.Http.Controllers.ReflectedHttpActionDescriptor+ActionExecutor.Execute(class System.Object,class System.Object[])", true)]
    [InlineData(@"Microsoft.Azure.ChangeAnalysis.AzureResourceManager.AzureResourceManagerClient.SendAuthenticatedRequestWithRetryAsync.AnonymousMethod__0(System.Net.Http.StreamContent streamContent, System.TimeSpan retryTimeout) Line 45 C#", true)]
    [InlineData(@"System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1[System.__Canon].Start(!!0&)", true)]
    [InlineData(@"System.Web.Http.Controllers.ApiControllerActionInvoker+<InvokeActionAsyncCore>d__1.MoveNext()", true)]

    [InlineData(@"SlowAllocateString", false)]
    public void ShouldMatchBasicCases(string input, bool expectMatch)
    {
        AIProfilerStackManagedSignatureMatcher target = new AIProfilerStackManagedSignatureMatcher(new AIProfilerStackFileInfoMatcher(Enumerable.Empty<ILineInfoMatcher>()));
        bool actual = target.TryCreate(input, string.Empty, FrameClassNameFactory.Instance, out _);
        Assert.Equal(expectMatch, actual);
    }

    [Fact]
    public void AssemblyInfoIsPassedThrough()
    {
        string input = @"System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1[System.__Canon].Start(!!0&)";
        string assemblyInfo = "HelloAssemblyInfo";
        AIProfilerStackManagedSignatureMatcher target = new AIProfilerStackManagedSignatureMatcher(new AIProfilerStackFileInfoMatcher(Enumerable.Empty<ILineInfoMatcher>()));
        bool actual = target.TryCreate(input, assemblyInfo, FrameClassNameFactory.Instance, out FrameItem? output);
        Assert.True(actual);
        Assert.NotNull(output);

        Assert.Equal(assemblyInfo, output?.AssemblySignature);
    }
}