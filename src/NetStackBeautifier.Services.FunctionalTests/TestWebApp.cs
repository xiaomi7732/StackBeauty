using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NetStackBeautifier.Services.Renders;

namespace NetStackBeautifier.Services.FunctionalTests;

internal class TestWebApp : WebApplicationFactory<WebAPI.Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Environment.SetEnvironmentVariable("CacheSettings:UseCache", "false");

        _ = builder.ConfigureTestServices(services =>
        {
            services.RegisterExceptionBeautifier();
            services.TryAddScoped<HtmlSectionRender>();
            services.TryAddScoped<HtmlRender>();
        });
        base.ConfigureWebHost(builder);
    }
}