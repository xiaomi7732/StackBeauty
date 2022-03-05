using System.Text;

namespace NetStackBeautifier.Services.Renders;

public class HtmlRender : IRender<string>
{
    private readonly HtmlSectionRender _mainDivRender;

    public HtmlRender(HtmlSectionRender mainDivRender)
    {
        _mainDivRender = mainDivRender ?? throw new ArgumentNullException(nameof(mainDivRender));
    }


    /// <summary>
    /// Renders iframe lines into a html.
    /// </summary>
    public async Task<string> RenderAsync(
        IReadOnlyCollection<IFrameLine> data,
        RenderOptions renderOptions,
        CancellationToken cancellationToken)
    {
        if (data.Count == 0)
        {
            return string.Empty;
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
                htmlBuilder.Append(await _mainDivRender.RenderAsync(data, renderOptions, cancellationToken).ConfigureAwait(false));
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

        return htmlBuilder.ToString();
    }


}