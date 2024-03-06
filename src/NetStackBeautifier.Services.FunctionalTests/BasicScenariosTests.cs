using System.Net.Http.Headers;
using System.Net.Mime;

namespace NetStackBeautifier.Services.FunctionalTests;

public class BasicScenariosTests
{
    [Fact]
    public async Task DemoScenario()
    {
        // Normal exception with call stack in it.
        string testInput = @"Unhandled exception. System.InvalidCastException: TestGenericsException
    at LearnThrow.Program.TestGenerics[T,T2](Action`1 target, String s) in D:\Demo\LearnThrow\Program.cs:line 49
    at LearnThrow.Program.Main(String[] args) in D:\Demo\LearnThrow\Program.cs:line 43
";

        await using var application = new TestWebApp();
        HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, "/beautified");
        HttpContent jsonContent = new StringContent(testInput, new MediaTypeHeaderValue(MediaTypeNames.Text.Plain));
        httpRequestMessage.Content = jsonContent;
        HttpClient client = application.CreateClient();


        HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

        Assert.True(response.IsSuccessStatusCode, $"Should return success code. Details: {response.ReasonPhrase}.");

        string result = await response.Content.ReadAsStringAsync();
        Assert.NotEqual("[]", result);
    }

    [Fact]
    public async Task DemoScenario2()
    {
        string testInput = $@"A!System.Private.CoreLib.System.Threading.ExecutionContext.RunInternal(class System.Threading.ExecutionContext,class System.Threading.ContextCallback,class System.Object)
A!Azure.Storage.Blobs.Azure.Storage.Blobs.BlobContainerClient+<CreateIfNotExistsAsync>d__58.MoveNext()
A!System.Private.CoreLib.System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1[System.__Canon].SetExistingTaskResult(class System.Threading.Tasks.Task`1<!0>,!0)
A!System.Private.CoreLib.System.Threading.Tasks.Task`1[System.__Canon].TrySetResult(!0)
A!System.Private.CoreLib.System.Threading.Tasks.Task.RunContinuations(class System.Object)
A!System.Private.CoreLib.System.Threading.Tasks.AwaitTaskContinuation.RunOrScheduleAction(class System.Action,bool)
A!System.Private.CoreLib.System.Runtime.CompilerServices.TaskAwaiter+<>c.<OutputWaitEtwEvents>b__12_0(class System.Action,class System.Threading.Tasks.Task)
A!System.Private.CoreLib.System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1+AsyncStateMachineBox`1[System.__Canon,ProfilerCaseStudy.SlowStorageAccess.Controllers.FastListBlobController+<GetAsync>d__3].MoveNext(class System.Threading.Thread)
A!System.Private.CoreLib.System.Threading.ExecutionContext.RunInternal(class System.Threading.ExecutionContext,class System.Threading.ContextCallback,class System.Object)
A!SlowStorageAccess.ProfilerCaseStudy.SlowStorageAccess.Controllers.FastListBlobController+<GetAsync>d__3.MoveNext()
A!System.Private.CoreLib.System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1[System.__Canon].SetExistingTaskResult(class System.Threading.Tasks.Task`1<!0>,!0)
A!System.Private.CoreLib.System.Threading.Tasks.Task`1[System.__Canon].TrySetResult(!0)
A!System.Private.CoreLib.System.Threading.Tasks.Task.RunContinuations(class System.Object)
A!System.Private.CoreLib.System.Threading.Tasks.AwaitTaskContinuation.RunOrScheduleAction(class System.Action,bool)
A!System.Private.CoreLib.System.Runtime.CompilerServices.TaskAwaiter+<>c.<OutputWaitEtwEvents>b__12_0(class System.Action,class System.Threading.Tasks.Task)
A!System.Private.CoreLib.System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1+AsyncStateMachineBox`1[System.__Canon,Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor+AwaitableObjectResultExecutor+<Execute>d__0].MoveNext(class System.Threading.Thread)
A!System.Private.CoreLib.System.Threading.ExecutionContext.RunInternal(class System.Threading.ExecutionContext,class System.Threading.ContextCallback,class System.Object)
A!Microsoft.AspNetCore.Mvc.Core.Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor+AwaitableObjectResultExecutor+<Execute>d__0.MoveNext()
";

        await using var application = new TestWebApp();
        HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, "/beautified");
        HttpContent jsonContent = new StringContent(testInput, new MediaTypeHeaderValue(MediaTypeNames.Text.Plain));
        httpRequestMessage.Content = jsonContent;
        HttpClient client = application.CreateClient();


        HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

        Assert.True(response.IsSuccessStatusCode, $"Should return success code. Details: {response.ReasonPhrase}.");

        string result = await response.Content.ReadAsStringAsync();
        Assert.NotEqual("[]", result);
    }
}