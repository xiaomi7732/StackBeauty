using NetStackBeautifier.Core;

namespace NetStackBeautifier.Services.StackBeautifiers;

/// <summary>
/// Aggregated service to create a FrameFileInfo object from content.
/// </summary>
public class AIProfilerStackFileInfoMatcher
{
    private readonly IEnumerable<ILineInfoMatcher> _matchers;

    public AIProfilerStackFileInfoMatcher(IEnumerable<ILineInfoMatcher> matchers)
    {
        _matchers = matchers ?? throw new ArgumentNullException(nameof(matchers));
    }

    public bool TryCreate(string content, out FrameFileInfo? frameFileInfo)
    {
        if (string.IsNullOrEmpty(content))
        {
            throw new ArgumentException($"'{nameof(content)}' cannot be null or empty.", nameof(content));
        }

        foreach (ILineInfoMatcher matcher in _matchers)
        {
            if (matcher.TryCreate(content, out FrameFileInfo? newInstance))
            {
                frameFileInfo = newInstance;
                return true;
            }
        }

        frameFileInfo = null;
        return false;
    }
}