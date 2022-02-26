using Microsoft.AspNetCore.Mvc;
using NetStackBeautifier.Services.Renders;
using System.Net;

namespace NetStackBeautifier.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class HtmlContentController : ControllerBase
{
    private readonly IRender<string> _render;

    public HtmlContentController(HtmlSectionRender render)
    {
        _render = render ?? throw new ArgumentNullException(nameof(render));
    }

    [HttpPost]
    public async Task<ContentResult> Render([FromBody] IReadOnlyCollection<IFrameLine> frames, CancellationToken cancellationToken)
    {
        return new ContentResult()
        {
            ContentType = "text/html",
            StatusCode = (int)HttpStatusCode.OK,
            Content = await _render.RenderAsync(frames, cancellationToken).ConfigureAwait(false),
        };
    }
}