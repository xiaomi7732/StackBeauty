using System.Runtime.CompilerServices;
using NetStackBeautifier.Core;
using NetStackBeautifier.Services.Filters;

namespace NetStackBeautifier.Services
{
    internal abstract class BeautifierBase<T> : IBeautifier
        where T : IBeautifier
    {
        private readonly LineBreaker _lineBreaker;
        private readonly IEnumerable<ILineBeautifier<T>> _lineBeautifiers;
        private readonly IEnumerable<IFrameFilter<T>> _filters;
        private readonly IEnumerable<IPreFilter<T>> _preFilters;

        public BeautifierBase(
            LineBreaker lineBreaker,
            IEnumerable<ILineBeautifier<T>> lineBeautifiers,
            IEnumerable<IFrameFilter<T>> filters,
            IEnumerable<IPreFilter<T>> preFilters)
        {
            _lineBreaker = lineBreaker ?? throw new ArgumentNullException(nameof(lineBreaker));
            _lineBeautifiers = lineBeautifiers ?? throw new ArgumentNullException(nameof(lineBeautifiers));
            _filters = filters ?? throw new ArgumentNullException(nameof(filters));
            _preFilters = preFilters ?? throw new ArgumentNullException(nameof(filters));
        }

        public async IAsyncEnumerable<IFrameLine> BeautifyAsync(
            string input,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            foreach (RawRequest rawRequest in _lineBreaker.BreakIntoLines(input).Select((value, i) => new RawRequest(value, i)))
            {
                // TODO: Revisit the logic to form hierarchy.
                IFrameLine newItem = CreateFrameItem(rawRequest.Value);
                RunPreFilters(newItem, rawRequest);

                yield return await BeautifyAsync(newItem, cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task<IFrameLine> BeautifyAsync(IFrameLine input, CancellationToken cancellationToken)
        {
            if (input is FrameItem frameItem)
            {
                foreach (ILineBeautifier<T> lineBeautifier in _lineBeautifiers)
                {
                    frameItem = await lineBeautifier.BeautifyAsync(frameItem, cancellationToken);
                }
                input = frameItem;
            }

            foreach (IFrameFilter<T> filter in _filters)
            {
                filter.Filter(input);
            }
            return input;
        }

        private void RunPreFilters(IFrameLine input, RawRequest rawRequest)
        {
            foreach (IPreFilter<T> filter in _preFilters)
            {
                filter.Filter(input, rawRequest);
            }
        }

        protected abstract IFrameLine CreateFrameItem(string line);

        public abstract bool CanBeautify(string input);
    }
}