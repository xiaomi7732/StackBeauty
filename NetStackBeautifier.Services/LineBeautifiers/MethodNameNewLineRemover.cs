using Microsoft.Extensions.Logging;
using NetStackBeautifier.Core;

namespace NetStackBeautifier.Services.LineBeautifiers;

internal class MethodNameNewLineRemover<T> : LineBeautifierBase<T>
    where T : BeautifierBase<T>
{
    public MethodNameNewLineRemover(ILogger<MethodNameNewLineRemover<T>> logger) : base(logger)
    {
    }

    protected override Task<FrameItem> BeautifyImpAsync(FrameItem line, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(line.Method?.Name))
        {
            return Task.FromResult(line);
        }

        line = line with
        {
            Method = line.Method with
            {
                Name = line.Method.Name.Replace("\r", "", StringComparison.Ordinal).Replace("\n", "", StringComparison.Ordinal)
            },
        };

        return Task.FromResult(line);
    }
}