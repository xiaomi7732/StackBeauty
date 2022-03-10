using Microsoft.AspNetCore.Mvc;
using NetStackBeautifier.Services.Renders;
using System.Net;

namespace NetStackBeautifier.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class HtmlController
{
    private readonly IRender<string> _render;

    public HtmlController(HtmlRender render)
    {
        _render = render ?? throw new ArgumentNullException(nameof(render));
    }

    [HttpPost]
    public async Task<ContentResult> Render(
        [FromBody] IReadOnlyCollection<IFrameLine> frames,
        [FromQuery] RenderMode renderMode = RenderMode.Simple,
        [FromQuery] bool preStyled = false,
        CancellationToken cancellationToken = default)
    {
        RenderOptions renderOptions = new RenderOptions
        {
            Mode = renderMode,
            PreStyled = preStyled
        };
        return new ContentResult()
        {
            ContentType = "text/html",
            StatusCode = (int)HttpStatusCode.OK,
            Content = await _render.RenderAsync(frames, renderOptions, cancellationToken).ConfigureAwait(false),
        };
    }
}