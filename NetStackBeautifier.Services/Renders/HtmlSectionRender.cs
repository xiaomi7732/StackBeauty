using System.Text;
using System.Web;
using NetStackBeautifier.Core;

namespace NetStackBeautifier.Services.Renders;

public class HtmlSectionRender : IRender<string>
{
    public Task<string> RenderAsync(
        IReadOnlyCollection<IFrameLine> data,
        RenderOptions renderOptions,
        CancellationToken cancellationToken)
    {
        StringBuilder htmlBuilder = new StringBuilder();
        htmlBuilder.AppendLine("<div>");
        try
        {
            foreach (IFrameLine line in data)
            {
                string lineToAdd = line switch
                {
                    FrameRawText rawText => RenderLine(rawText),
                    FrameItem item => RenderLine(item, renderOptions),
                    _ => throw new NotSupportedException(),
                };
                // Nothing to append when nothing returned.
                if (string.IsNullOrEmpty(lineToAdd))
                {
                    continue;
                }
                htmlBuilder.AppendLine(lineToAdd);
            }
        }
        finally
        {
            htmlBuilder.AppendLine("</div>");
        }

        return Task.FromResult(htmlBuilder.ToString());
    }

    private string RenderLine(FrameRawText rawText)
    {
        return $"<div class='description-text'>{HttpUtility.HtmlEncode(rawText.Value)}</div>";
    }

    private string RenderLine(FrameItem frameItem, RenderOptions renderOptions)
    {
        // When this line is recognized as noise and the render mode is simple, skip this line by returning string.Empty;
        if (frameItem.Tags.Contains(new KeyValuePair<string, string>("Noise", "true")) && renderOptions.Mode == RenderMode.Simple)
        {
            return string.Empty;
        }

        if (frameItem.Method is null)
        {
            throw new ArgumentException("FrameItem.Method is required.");
        }

        return $@"<div class='frame-line-container'>
<span class='frame-class-name' title='{HttpUtility.HtmlEncode(frameItem.FullClass?.FullClassNameOrDefault + frameItem.AssemblySignature)} || Tracking Id:{HttpUtility.HtmlEncode(frameItem.Id)}'>{HttpUtility.HtmlEncode(frameItem.FullClass?.ShortClassNameOrDefault ?? frameItem.AssemblySignature)}</span>{RenderClassGenericTypes(frameItem.FullClass?.GenericParameterTypes)}.<span class='frame-method-name'>{HttpUtility.HtmlEncode(frameItem.Method.Name)}</span>{RenderGenericMethodTypes(frameItem.Method)}{RenderParameterList(frameItem.Method.Parameters.NullAsEmpty().ToList().AsReadOnly())}<span>&nbsp;{{ ... }}</span>
{Render(frameItem.FileInfo)}
</div>";
    }

    private string RenderGenericMethodTypes(FrameMethod method)
    {
        if (!method.GenericParameterTypes.NullAsEmpty().Any())
        {
            return string.Empty;
        }

        string typeList = string.Join(", ", method.GenericParameterTypes);

        return $"<span class='frame-method-generic-parameters'>{HttpUtility.HtmlEncode('<' + typeList + '>')}</span>";
    }

    private string RenderClassGenericTypes(IEnumerable<string>? genericParameterTypes)
    {
        if (!genericParameterTypes.NullAsEmpty().Any())
        {
            return string.Empty;
        }

        string line = string.Empty;
        foreach (string typeName in genericParameterTypes.NullAsEmpty())
        {
            line += $"{typeName}, ";
        }
        line = HttpUtility.HtmlEncode("<" + line.Substring(0, line.Length - ", ".Length) + ">");

        return $"<span class='frame-class-generic-parameter-list'>{line}</span>";
    }

    private string RenderParameterList(IReadOnlyCollection<FrameParameter> parameters)
    {
        if (parameters.Count == 0)
        {
            return $"<span class='frame-parameter-list-empty'>(..)</span>";
        }

        string parameterList = string.Empty;
        foreach (FrameParameter p in parameters)
        {
            parameterList += $"<span class='frame-parameter-type'>{HttpUtility.HtmlEncode(p.ParameterType)}</span>&nbsp;<span class='frame-parameter-name'>{HttpUtility.HtmlEncode(p.ParameterName)}</span>,&nbsp;";
        }

        return $"<span class='frame-parameter-list'>({parameterList.Substring(0, parameterList.Length - 7)})</span>";
    }

    private string Render(FrameFileInfo? fileInfo)
    {
        if (fileInfo is null)
        {
            return string.Empty;
        }

        return $"<span class='frame-file-path'>&nbsp;&nbsp;&nbsp;&nbsp;{HttpUtility.HtmlEncode(fileInfo.FilePath)}</span>#<span class='frame-file-line'>{HttpUtility.HtmlEncode(fileInfo.LineNumber)}</span>";
    }
}