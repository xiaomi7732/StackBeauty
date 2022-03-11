using System.Text;
using System.Web;
using System.Collections.Generic;
using NetStackBeautifier.Core;
using Markdig;

namespace NetStackBeautifier.Services.Renders;

public class HtmlSectionRender : IRender<string>
{
    private const string AnalysisMarkDownKey = "AnalysisMarkDown";
    private Dictionary<string, string> classToStyleMap;

    public HtmlSectionRender()
    {
        classToStyleMap = new Dictionary<string, string>
        {
            { "frame-class-name", "color: #996800;" },
            { "frame-method-name", "font-weight: bold;color: #2271B1;" },
            { "frame-parameter-list", "color: #362400;" },
            { "frame-file-path", "color: #336B87;" },
            { "frame-file-line", "color: #D63638;" },
            { "frame-parameter-type", "font-weight: bold;color: #043959;" }
        };
    }


    public Task<string> RenderAsync(
        IReadOnlyCollection<IFrameLine> data,
        RenderOptions renderOptions,
        CancellationToken cancellationToken)
    {
        StringBuilder htmlBuilder = new StringBuilder();
        htmlBuilder.AppendLine("<div class='frame-content'>");

        int beginPos = htmlBuilder.Length - 1;
        string analysisMarkdown = string.Empty;
        try
        {
            htmlBuilder.AppendLine("<div class='frame-lines'>");
            try
            {
                foreach (IFrameLine line in data)
                {
                    // Skip noise
                    if (SkipLine(line, renderOptions))
                    {
                        continue;
                    }

                    if (line.Tags.ContainsKey(AnalysisMarkDownKey))
                    {
                        analysisMarkdown = line.Tags[AnalysisMarkDownKey];
                    }

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

            // Append analysis markdown
            if (!string.IsNullOrEmpty(analysisMarkdown))
            {
                htmlBuilder.Insert(beginPos, RenderAnalysis(analysisMarkdown));
            }
        }
        finally
        {
            htmlBuilder.AppendLine("</div>");
        }

        return Task.FromResult(htmlBuilder.ToString());
    }

    private string RenderAnalysis(string markdown)
    {
        return $"<div class='analysis-markdown-container'>{Markdown.ToHtml(markdown)}</div>";
    }

    /// <summary>
    /// Skips lines that is noise when rendering in simple mode.
    /// </summary>
    private bool SkipLine(IFrameLine line, RenderOptions renderOptions) => line.Tags.Contains(new KeyValuePair<string, string>("noise", "true")) && renderOptions.Mode == RenderMode.Simple;

    private string RenderLine(FrameRawText rawText)
    {
        return $"<div class='description-text'>{HttpUtility.HtmlEncode(rawText.Value)}</div>";
    }

    private string RenderLine(FrameItem frameItem, RenderOptions renderOptions)
    {
        if (frameItem.Method is null)
        {
            throw new ArgumentException("FrameItem.Method is required.");
        }

        return $@"<div class='frame-line-container'>
<span{GenerateAttributes("frame-class-name", renderOptions.PreStyled)} title='{HttpUtility.HtmlEncode(frameItem.FullClass?.FullClassNameOrDefault + frameItem.AssemblySignature)} || Tracking Id:{HttpUtility.HtmlEncode(frameItem.Id)}'>{HttpUtility.HtmlEncode(frameItem.FullClass?.ShortClassNameOrDefault ?? frameItem.AssemblySignature)}</span>{RenderClassGenericTypes(frameItem.FullClass?.GenericParameterTypes, renderOptions.PreStyled)}.<span{GenerateAttributes("frame-method-name", renderOptions.PreStyled)}>{HttpUtility.HtmlEncode(frameItem.Method.Name)}</span>{RenderGenericMethodTypes(frameItem.Method, renderOptions.PreStyled)}{RenderParameterList(frameItem.Method.Parameters.NullAsEmpty().ToList().AsReadOnly(), renderOptions.PreStyled)}{RenderBodyHolder()}
{Render(frameItem.FileInfo, renderOptions.PreStyled)}
</div>";
    }

    private string GenerateAttributes(string className, bool isDefault)
    {
        Dictionary<string, string> attributes = new Dictionary<string, string>
        {
            { "class", className}
        };

        string? style;
        bool classExists = classToStyleMap.TryGetValue(className, out style);
        if (isDefault && classExists && style is not null)
        {
            attributes.Add("style", style);
        }

        string attributeString = "";
        foreach (KeyValuePair<string, string> attribute in attributes)
        {
            attributeString += $" {attribute.Key}='{attribute.Value}'";
        }

        return attributeString;
    }

    private string RenderGenericMethodTypes(FrameMethod method, bool isDefault)
    {
        string className = "frame-method-generic-parameters";
        if (!method.GenericParameterTypes.NullAsEmpty().Any())
        {
            return string.Empty;
        }

        string typeList = string.Join(", ", method.GenericParameterTypes);

        return $"<span{GenerateAttributes(className, isDefault)}>{HttpUtility.HtmlEncode('<' + typeList + '>')}</span>";
    }

    private string RenderClassGenericTypes(IEnumerable<string>? genericParameterTypes, bool isDefault)
    {
        string className = "frame-class-generic-parameter-list";
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

        return $"<span{GenerateAttributes(className, isDefault)}>{line}</span>";
    }

    private string RenderParameterList(IReadOnlyCollection<FrameParameter> parameters, bool isDefault)
    {
        if (parameters.Count == 0)
        {
            return $"<span class='frame-parameter-list-empty'>(..)</span>";
        }

        string parameterList = string.Empty;
        foreach (FrameParameter p in parameters)
        {
            parameterList += $"<span{GenerateAttributes("frame-parameter-type", isDefault)}>{HttpUtility.HtmlEncode(p.ParameterType)}</span>&nbsp;<span class='frame-parameter-name'>{HttpUtility.HtmlEncode(p.ParameterName)}</span>,&nbsp;";
        }

        return $"<span{GenerateAttributes("frame-parameter-list", isDefault)}>({parameterList.Substring(0, parameterList.Length - 7)})</span>";
    }

    private string Render(FrameFileInfo? fileInfo, bool isDefault)
    {
        if (fileInfo is null)
        {
            return string.Empty;
        }

        return $"<span{GenerateAttributes("frame-file-path", isDefault)}>&nbsp;&nbsp;&nbsp;&nbsp;{HttpUtility.HtmlEncode(fileInfo.FilePath)}</span>#<span{GenerateAttributes("frame-file-line", isDefault)}>{HttpUtility.HtmlEncode(fileInfo.LineNumber)}</span>";
    }

    private string RenderBodyHolder()
    {
        return $"<span class='frame-method-body-holder'>{{ .. }}</span>";
    }
}