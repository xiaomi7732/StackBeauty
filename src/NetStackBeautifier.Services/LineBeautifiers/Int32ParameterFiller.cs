using Microsoft.Extensions.Logging;
using NetStackBeautifier.Core;

namespace NetStackBeautifier.Services.LineBeautifiers;

internal class Int32ParameterFiller<T> : LineBeautifierBase<T>
    where T : IBeautifier
{
    public Int32ParameterFiller(ILogger<Int32ParameterFiller<T>> logger) : base(logger)
    {
    }

    protected override Task<FrameItem> BeautifyImpAsync(FrameItem line, CancellationToken cancellationToken = default)
    {
        if (line.Method is null || !line.Method.Parameters.NullAsEmpty().Any())
        {
            return Task.FromResult(line);
        }

        List<FrameParameter> newParamList = new List<FrameParameter>();
        int order = 1;
        foreach (FrameParameter parameter in line.Method.Parameters.NullAsEmpty())
        {
            if (string.Equals(parameter.ParameterType, "int32", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(parameter.ParameterType, "int", StringComparison.OrdinalIgnoreCase))
            {
                string paramName = parameter.ParameterName;
                if (string.IsNullOrEmpty(paramName))
                {
                    paramName = "n" + order++;
                }
                newParamList.Add(new FrameParameter("int", paramName));
                continue;
            }

            newParamList.Add(parameter);
        }

        return Task.FromResult(line with
        {
            Method = line.Method with {
                Parameters = newParamList,
            },
        });
    }
}