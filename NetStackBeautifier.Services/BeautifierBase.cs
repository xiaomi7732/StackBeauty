using System.Runtime.CompilerServices;
using NetStackBeautifier.Core;

namespace NetStackBeautifier.Services
{
    internal abstract class BeautifierBase<T> : IBeautifier
    {
        private readonly LineBreaker _lineBreaker;
        private readonly IEnumerable<ILineBeautifier<T>> _lineBeautifiers;

        public BeautifierBase(
            LineBreaker lineBreaker,
            IEnumerable<ILineBeautifier<T>> lineBeautifiers)
        {
            _lineBreaker = lineBreaker ?? throw new ArgumentNullException(nameof(lineBreaker));
            _lineBeautifiers = lineBeautifiers ?? throw new ArgumentNullException(nameof(lineBeautifiers));
        }

        public async IAsyncEnumerable<IFrameLine> BeautifyAsync(
            string input,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            IFrameLine? firstNode = null;
            foreach (string line in _lineBreaker.BreakIntoLines(input))
            {
                // TODO: Revisit the logic to form hierarchy.
                IFrameLine newItem = CreateFrameItem(line);
                if (firstNode is null)
                {
                    firstNode = newItem;
                }
                else
                {
                    firstNode.Children.Add(newItem.Id);
                }

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
            return input;
        }

        protected abstract IFrameLine CreateFrameItem(string line);

        public abstract bool CanBeautify(string input);
    }
}