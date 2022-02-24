namespace NetStackBeautifier.Services;

public interface IBeautifierService
{
    IAsyncEnumerable<IFrameLine> BeautifyAsync(string input, CancellationToken cancellationToken);
}