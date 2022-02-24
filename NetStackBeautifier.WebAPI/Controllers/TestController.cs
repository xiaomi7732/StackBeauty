using Microsoft.AspNetCore.Mvc;
using NetStackBeautifier.Services;

namespace NetStackBeautifier.WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        private readonly IBeautifierService _beautifierService;

        public TestController(IBeautifierService beautifierService)
        {
            _beautifierService = beautifierService;
        }

        [HttpGet]
        public async IAsyncEnumerable<object> Get(CancellationToken cancellationToken)
        {
            string input = @"Unhandled exception. System.InvalidCastException: TestGenericsException
   at Program.<<Main>$>g__TestGenerics|0_0[T,T2](Action`1 target, String s) in D:\Demo\LearnThrow\Program.cs:line 17
   at Program.<Main>$(String[] args) in D:\Demo\LearnThrow\Program.cs:line 3
";
            await foreach(IFrameLine line in _beautifierService.BeautifyAsync(input, cancellationToken))
            {
                yield return line;
            }
        }
    }
}