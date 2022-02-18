using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using NetStackBeautifier.Core;

namespace NetStackBeautifier.Services;
internal abstract class BeautifierService
{
    private readonly IEnumerable<IBeautifier> _beautifiers;
    private readonly ILogger<BeautifierService> _logger;

    public BeautifierService(
        IEnumerable<IBeautifier> beautifiers,
        ILogger<BeautifierService> logger)
    {
        _beautifiers = beautifiers ?? throw new ArgumentNullException(nameof(beautifiers));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async IAsyncEnumerable<FrameItem> BeautifyAsync(string input, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        _logger.LogInformation("Picking target beautifier");
        foreach (IBeautifier beautifier in _beautifiers)
        {
            if (beautifier.CanBeautify(input))
            {
                _logger.LogInformation("First matched beautifier: {beautifierName}", beautifier.GetType().Name);
                await foreach (FrameItem item in beautifier.BeautifyAsync(input, cancellationToken))
                {
                    yield return item;
                }
            }
        }
    }
}