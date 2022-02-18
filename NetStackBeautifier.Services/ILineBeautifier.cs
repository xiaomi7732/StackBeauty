using NetStackBeautifier.Core;

namespace NetStackBeautifier.Services
{
    public interface ILineBeautifier
    {
        Task<FrameItem> BeautifyAsync(FrameItem line, CancellationToken cancellationToken = default);
    }

    public interface ILineBeautifier<T>
        : ILineBeautifier
    {
    }
}