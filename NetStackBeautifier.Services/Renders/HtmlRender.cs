using System.Text;
using System.Web;
using NetStackBeautifier.Core;

namespace NetStackBeautifier.Services.Renders;

public class HtmlRender : IRender<string>
{

    /// <summary>
    /// Renders iframe lines into a html.
    /// </summary>
    public Task<string> RenderAsync(IReadOnlyCollection<IFrameLine> data, CancellationToken cancellationToken)
    {
        if (data.Count == 0)
        {
            return Task.FromResult(string.Empty);
        }

        StringBuilder htmlBuilder = new StringBuilder();
        htmlBuilder.AppendLine("<!doctype html>");
        htmlBuilder.AppendLine("<html lang='en'>");
        try
        {
            htmlBuilder.AppendLine("<header>");
            try
            {
                htmlBuilder.AppendLine("<meta charset='utf-8'>");
                htmlBuilder.AppendFormat("<title>{0}</title>", ".NET Trace Beautifier");
                htmlBuilder.AppendLine();
                htmlBuilder.AppendLine(@"<style>
    body {
        font-family: Verdana,sans-serif;
        font-size: 14px;
        line-height: 1.5em;
    }

    .description-text {
    }

    .frame-class-name {
        color: #996800;
    }

    .frame-method-name {
        font-weight: bold;
        color: #2271B1;
    }

    .frame-parameter-list {
        color: #362400;
    }

    .frame-file-path {
        color: #336B87;
    }

    .frame-file-line {
        color: #D63638;
    }

    .frame-parameter-type {
        color: #043959 ;
        font-weight: bold;
    }
</style>");
            }
            finally
            {
                htmlBuilder.AppendLine("</header>");
            }

            htmlBuilder.AppendLine("<body>");
            try
            {
                foreach (IFrameLine line in data)
                {
                    string lineToAdd = line switch
                    {
                        FrameRawText rawText => RenderLine(rawText),
                        FrameItem item => RenderLine(item),
                        _ => throw new NotSupportedException(),
                    };
                    htmlBuilder.AppendLine(lineToAdd);
                }
            }
            finally
            {
                htmlBuilder.AppendLine("</body>");
            }
        }
        finally
        {
            htmlBuilder.AppendLine("</html>");
        }

        return Task.FromResult(htmlBuilder.ToString());
    }

    private string RenderLine(FrameRawText rawText)
    {
        return $"<div class='description-text'>{HttpUtility.HtmlEncode(rawText.Value)}</div>";
    }

    private string RenderLine(FrameItem frameItem)
    {
        if (frameItem.FullClass is null)
        {
            throw new ArgumentException("Full className is required.");
        }

        if (frameItem.Method is null)
        {
            throw new ArgumentException("FrameItem.Method is required.");
        }

        return $@"<div>
<span class='frame-class-name' title='{HttpUtility.HtmlEncode(frameItem.FullClass.FullClassNameOrDefault)} {HttpUtility.HtmlEncode(frameItem.Id)}'>{HttpUtility.HtmlEncode(frameItem.FullClass.ShortClassNameOrDefault)}</span>{RenderClassGenericTypes(frameItem.FullClass.GenericParameterTypes)}.<span class='frame-method-name'>{HttpUtility.HtmlEncode(frameItem.Method.Name)}</span>{RenderParameterList(frameItem.Method.Parameters.NullAsEmpty().ToList().AsReadOnly())}<span>{{}}</span>
{Render(frameItem.FileInfo)}
</div>";
    }

    private string RenderClassGenericTypes(IEnumerable<string> genericParameterTypes)
    {
        if (!genericParameterTypes.NullAsEmpty().Any())
        {
            return string.Empty;
        }

        string line = string.Empty;
        foreach (string typeName in genericParameterTypes)
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
            return string.Empty;
        }

        string parameterList = string.Empty;
        foreach (FrameParameter p in parameters)
        {
            parameterList += $"<span class='frame-parameter-type'>{HttpUtility.HtmlEncode(p.ParameterType)}</span>&nbsp;<span>{HttpUtility.HtmlEncode(p.ParameterName)}</span>,&nbsp;";
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