using Microsoft.Extensions.Logging;
using NetStackBeautifier.Core;

namespace NetStackBeautifier.Services.LineBeautifiers;

internal class ClassGenericTypeSystemCanonicalBeautifier<T> : LineBeautifierBase<T>
    where T : BeautifierBase<T>
{
    public ClassGenericTypeSystemCanonicalBeautifier(
        ILogger<ClassGenericTypeSystemCanonicalBeautifier<T>> logger) : base(logger)
    {
    }

    protected override Task<FrameItem> BeautifyImpAsync(FrameItem line, CancellationToken cancellationToken = default)
    {
        if (line.FullClass is null)
        {
            return Task.FromResult(line);
        }

        if (!line.FullClass.GenericParameterTypes.NullAsEmpty().Any())
        {
            return Task.FromResult(line);
        }

        int tCount = 0;

        List<string> result = new List<string>();
        List<string> rawResult = new List<string>();
        foreach (string item in line.FullClass.GenericParameterTypes)
        {
            rawResult.Add(item);
            if (string.Equals(item, "System.__Canon", StringComparison.OrdinalIgnoreCase))
            {
                result.Add(GetT(++tCount));
            }
            else
            {
                result.Add(item);
            }
        }

        line = line with
        {
            FullClass = line.FullClass with
            {
                GenericParameterTypes = result,
                RawGenericParameterTypes = rawResult
            },
        };

        return Task.FromResult(line);
    }

    private string GetT(int order)
    {
        if (order == 1)
        {
            return "T";
        }
        return $"T{order}";
    }
}