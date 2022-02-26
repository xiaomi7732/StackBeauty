using System.Runtime.CompilerServices;
using NetStackBeautifier.Core;
using NetStackBeautifier.Services.Filters;

namespace NetStackBeautifier.Services
{
    internal abstract class BeautifierBase<T> : IBeautifier
        where T: IBeautifier
    {
        private readonly LineBreaker _lineBreaker;
        private readonly IEnumerable<ILineBeautifier<T>> _lineBeautifiers;
        private readonly IEnumerable<IFrameFilter<T>> _filters;

        public BeautifierBase(
            LineBreaker lineBreaker,
            IEnumerable<ILineBeautifier<T>> lineBeautifiers,
            IEnumerable<IFrameFilter<T>> filters)
        {
            _lineBreaker = lineBreaker ?? throw new ArgumentNullException(nameof(lineBreaker));
            _lineBeautifiers = lineBeautifiers ?? throw new ArgumentNullException(nameof(lineBeautifiers));
            _filters = filters ?? throw new ArgumentNullException(nameof(filters));
        }

        public async IAsyncEnumerable<IFrameLine> BeautifyAsync(
            string input,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            foreach (string line in _lineBreaker.BreakIntoLines(input))
            {
                // TODO: Revisit the logic to form hierarchy.
                IFrameLine newItem = CreateFrameItem(line);
                yield return newItem switch
                {
                    (FrameRawText rawText) => rawText,
                    (FrameItem frameItem) => await BeautifyAsync(frameItem, cancellationToken),
                    _ => throw new InvalidOperationException("Unsupported frame item type."),
                };
            }
        }

        private async Task<FrameItem> BeautifyAsync(FrameItem input, CancellationToken cancellationToken)
        {
            foreach (ILineBeautifier<T> lineBeautifier in _lineBeautifiers)
            {
                input = await lineBeautifier.BeautifyAsync(input, cancellationToken);
            }
            foreach(IFrameFilter<T> filter in _filters)
            {
                filter.Filter(input);
            }
            return input;
        }

        protected abstract IFrameLine CreateFrameItem(string line);

        public abstract bool CanBeautify(string input);
    }
}