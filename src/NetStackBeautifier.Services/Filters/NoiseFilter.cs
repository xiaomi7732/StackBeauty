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
            string.Equals(frameItem.Method?.Name, "Throw"))
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

        return false;
    }
}