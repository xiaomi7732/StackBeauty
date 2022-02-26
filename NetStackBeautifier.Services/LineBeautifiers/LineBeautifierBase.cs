using System.Diagnostics;
using Microsoft.Extensions.Logging;
using NetStackBeautifier.Core;

namespace NetStackBeautifier.Services.LineBeautifiers
{
    internal abstract class LineBeautifierBase<T> : ILineBeautifier<T>
        where T : IBeautifier
    {
        private readonly ILogger<LineBeautifierBase<T>> _logger;

        public LineBeautifierBase(ILogger<LineBeautifierBase<T>> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<FrameItem> BeautifyAsync(FrameItem line, CancellationToken cancellationToken = default)
        {
            Stopwatch watch = new Stopwatch();
            string beautifierName = GetType().Name;
            try
            {
                watch.Start();
                _logger.LogInformation("Start Beautifier: {beautifierName}", beautifierName);
                return await BeautifyImpAsync(line, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                watch.Stop();
                _logger.LogInformation("Stop Beautifier: {beautifierName}. Elapsed: {elapsed} ms.", beautifierName, watch.ElapsedMilliseconds);
            }
        }

        protected abstract Task<FrameItem> BeautifyImpAsync(FrameItem line, CancellationToken cancellationToken = default);
    }
}