using System.Runtime.CompilerServices;

namespace NetStackBeautifier.Services
{
    internal class DumbBeautifier : IBeautifier
    {
        public async IAsyncEnumerable<IFrameLine> BeautifyAsync(
            string input,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            yield break;
        }

        public bool CanBeautify(string input)
        {
            return false;
        }
    }
}