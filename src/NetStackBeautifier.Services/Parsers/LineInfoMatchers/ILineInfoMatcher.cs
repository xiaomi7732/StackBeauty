using NetStackBeautifier.Core;

namespace NetStackBeautifier.Services.StackBeautifiers;

/// <summary>
/// Services that matches line info from a stack frame
/// </summary>
public interface ILineInfoMatcher
{
    bool TryCreate(string input, out FrameFileInfo? fileInfo);
}