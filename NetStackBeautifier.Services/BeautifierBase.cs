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

        public async IAsyncEnumerable<FrameItem> BeautifyAsync(
            string input,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            foreach (string line in _lineBreaker.BreakIntoLines(input))
            {
                FrameItem newItem = CreateFrameItem(line);
                foreach (ILineBeautifier<T> lineBeautifier in _lineBeautifiers)
                {
                    newItem = await lineBeautifier.BeautifyAsync(newItem, cancellationToken);
                }
                yield return newItem;
            }
        }

        protected abstract FrameItem CreateFrameItem(string line);

        public abstract bool CanBeautify(string input);
    }
}