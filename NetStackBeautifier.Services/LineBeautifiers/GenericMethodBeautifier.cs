using System.Text.RegularExpressions;
using NetStackBeautifier.Core;

namespace NetStackBeautifier.Services.LineBeautifiers
{
    /// <summary>
    /// Beautify generic method.
    /// For example, from: TestGenerics[T,T2] => TestGenerics<T,T2>
    /// </summary>
    internal class GenericMethodBeautifier<T> : ILineBeautifier<T>
        where T : BeautifierBase<T>
    {
        private const string GenericMatcherExpression = @"(.*)\[(.*)\]";
        private static readonly Regex matcher = new Regex(GenericMatcherExpression, RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));

        public Task<FrameItem> BeautifyAsync(FrameItem line, CancellationToken cancellationToken = default)
        {
            string? methodContent = line.Method?.Name;

            // Short hand, no mehtod to deal with.
            if (string.IsNullOrEmpty(methodContent))
            {
                return Task.FromResult(line);
            }

            // Group[1]: Method Name; Group[2]: Type-parameter list.
            Match match = matcher.Match(methodContent);
            if (match.Success && match.Groups.Count == 3)
            {
                line = line with
                {
                    Method = line.Method! with
                    {
                        Name = $"{match.Groups[1].Value}<{match.Groups[2].Value.Replace(",", ", ").Replace("  ", " ")}>",
                    },
                };
            }

            return Task.FromResult(line);
        }
    }
}