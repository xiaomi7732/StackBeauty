using System.Text;
using System.Web;
using NetStackBeautifier.Core;

namespace NetStackBeautifier.Services.Renders;

public class HtmlSectionRender : IRender<string>
{
    public Task<string> RenderAsync(IReadOnlyCollection<IFrameLine> data, CancellationToken cancellationToken)
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
                    FrameItem item => RenderLine(item),
                    _ => throw new NotSupportedException(),
                };
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