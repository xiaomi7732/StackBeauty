using Microsoft.Extensions.Logging;
using NetStackBeautifier.Core;

namespace NetStackBeautifier.Services.LineBeautifiers;

[Obsolete("Not needed anymore", error: true)]
internal class AzureProfilerMethodEndingBeautifier<T> : LineBeautifierBase<T>
    where T : BeautifierBase<T>
{
    public AzureProfilerMethodEndingBeautifier(
        ILogger<AzureProfilerMethodEndingBeautifier<T>> logger) : base(logger)
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
                Name = line.Method.Name + "()",
            },
        };

        return Task.FromResult(line);
    }
}
