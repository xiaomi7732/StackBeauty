using Microsoft.AspNetCore.Mvc;
using NetStackBeautifier.Services;
using System.Runtime.CompilerServices;

namespace NetStackBeautifier.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class BeautifiedController : ControllerBase
{
    private readonly IBeautifierService _beautifierService;

    public BeautifiedController(IBeautifierService beautifierService)
    {
        _beautifierService = beautifierService ?? throw new ArgumentNullException(nameof(beautifierService));
    }

    [HttpPost]
    public IAsyncEnumerable<IFrameLine> Post([FromBody] string body, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(body))
        {
            throw new ArgumentNullException(nameof(body));
        }
        return _beautifierService.BeautifyAsync(body, cancellationToken);
    }
}