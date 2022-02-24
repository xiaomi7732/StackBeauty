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
    public async IAsyncEnumerable<object> Post([FromBody] string body, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(body))
        {
            throw new ArgumentNullException(nameof(body));
        }

        await foreach (IFrameLine line in _beautifierService.BeautifyAsync(body, cancellationToken))
        {
            yield return line;
        }
    }
}