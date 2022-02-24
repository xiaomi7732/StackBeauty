using NetStackBeautifier.Core;

namespace NetStackBeautifier.Services
{
    /// <summary>
    /// A service that beautifies a callstack.
    /// </summary>
    public interface IBeautifier
    {
        /// <summary>
        /// Checks wheather the current input could be beautified.
        /// </summary>
        bool CanBeautify(string input);

        /// <summary>
        /// Executes the beautify on a whole stack.
        /// </summary>
        IAsyncEnumerable<IFrameLine> BeautifyAsync(string input, CancellationToken cancellationToken = default);
    }
}