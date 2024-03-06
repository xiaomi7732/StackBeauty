using System.Runtime.InteropServices;
using NetStackBeautifier.Core;

namespace NetStackBeautifier.Services.Filters;

internal class NoiseFilter<T> : IFrameFilter<T>
    where T : IBeautifier
{
    public void Filter(IFrameLine frameline)
    {
        bool tagNoise = (frameline) switch
        {
            FrameRawText frameRawText => IsNoise(frameRawText),
            FrameItem frameItem => IsNoise(frameItem),
            _ => throw new NotSupportedException($"Unsupported IFrameLine type: {frameline.GetType()}")
        };

        if (tagNoise)
        {
            frameline.Tags.TryAdd(KnownTagKey.Noise, "true");
        }
    }

    private bool IsNoise(FrameItem frameItem)
    {
        // Make a noisy check prvoiders collection from this

        if (frameItem is null)
        {
            return false;
        }

        if (string.Equals(frameItem.FullClass?.FullClassNameOrDefault, "System.Runtime.CompilerServices.TaskAwaiter", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(frameItem.Method?.Name, "HandleNonSuccessAndDebuggerNotification", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (string.Equals(frameItem.FullClass?.FullClassNameOrDefault, "System.Runtime.ExceptionServices.ExceptionDispatchInfo", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(frameItem.Method?.Name, "Throw", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (string.Equals(frameItem.FullClass?.FullClassNameOrDefault, "System.Private.CoreLib.System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (string.Equals(frameItem.FullClass?.FullClassNameOrDefault, "System.Private.CoreLib.System.Threading.ExecutionContext", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (!string.IsNullOrEmpty(frameItem.FullClass?.FullClassNameOrDefault) && frameItem.FullClass.FullClassNameOrDefault.StartsWith("System.Private.CoreLib.System.Threading.Tasks.Task", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(frameItem.Method?.Name, "TrySetResult", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (string.Equals(frameItem.FullClass?.FullClassNameOrDefault, "System.Private.CoreLib.System.Threading.Tasks.Task", StringComparison.OrdinalIgnoreCase)
            && string.Equals(frameItem.Method?.Name, "RunContinuations"))
        {
            return true;
        }

        if(string.Equals(frameItem.FullClass?.FullClassNameOrDefault, "System.Private.CoreLib.System.Threading.Tasks.AwaitTaskContinuation", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if(!string.IsNullOrEmpty(frameItem.FullClass?.FullClassNameOrDefault) && frameItem.FullClass.FullClassNameOrDefault.StartsWith("System.Private.CoreLib.System.Runtime.CompilerServices.TaskAwaiter", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if(!string.IsNullOrEmpty(frameItem.FullClass?.FullClassNameOrDefault) && frameItem.FullClass.FullClassNameOrDefault.StartsWith("System.Private.CoreLib.System.Runtime.CompilerServices.AsyncTaskMethodBuilder", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if(!string.IsNullOrEmpty(frameItem.FullClass?.FullClassNameOrDefault) && frameItem.FullClass.FullClassNameOrDefault.StartsWith("Microsoft.AspNetCore.Mvc.Core.Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

    private bool IsNoise(FrameRawText rawText)
    {
        if (string.IsNullOrEmpty(rawText?.Value))
        {
            return false;
        }

        if (rawText.Value.StartsWith("Missing symbol in assembly [", StringComparison.Ordinal))
        {
            return true;
        }

        if (rawText.Value.Trim().Equals("[Resuming Async Method]", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (rawText.Value.Trim().StartsWith("[External Code]", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (rawText.Value.Trim().Equals("[Async Call Stack]", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }
}