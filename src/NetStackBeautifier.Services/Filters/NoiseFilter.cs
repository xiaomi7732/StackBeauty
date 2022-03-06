using NetStackBeautifier.Core;

namespace NetStackBeautifier.Services.Filters;

internal class NoiseFilter<T> : IFrameFilter<T>
    where T : IBeautifier
{
    public void Filter(IFrameLine frameline)
    {
        if (frameline is FrameRawText)
        {
            return;
        }

        bool tagNoise = false;
        if (frameline is FrameItem frameItem)
        {
            if (string.Equals(frameItem.FullClass?.FullClassNameOrDefault, "System.Runtime.CompilerServices.TaskAwaiter", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(frameItem.Method?.Name, "HandleNonSuccessAndDebuggerNotification", StringComparison.OrdinalIgnoreCase))
            {
                tagNoise = true;
            }
            else if (
                string.Equals(frameItem.FullClass?.FullClassNameOrDefault, "System.Runtime.ExceptionServices.ExceptionDispatchInfo", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(frameItem.Method?.Name, "Throw")
            )
            {
                tagNoise = true;
            }


            if (tagNoise)
            {
                frameItem.Tags.TryAdd("noise", "true");
            }
        }
    }
}