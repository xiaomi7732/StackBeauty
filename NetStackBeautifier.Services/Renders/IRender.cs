namespace NetStackBeautifier.Services.Renders;

public interface IRender<T>
{
    Task<T> RenderAsync(IReadOnlyCollection<IFrameLine> data, CancellationToken cancellationToken);
}