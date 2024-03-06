using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NetStackBeautifier.Core;
using NetStackBeautifier.Services;
using NetStackBeautifier.Services.Renders;
using NetStackBeautifier.WebAPI;

namespace NetStackBeautifier.WebAPI;

public class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddHealthChecks();
        builder.Services.AddApplicationInsightsTelemetry();
        builder.Services.AddControllers(options =>
        {
            options.InputFormatters.Insert(0, new TextMediaTypeFormatter());
        }).AddJsonOptions(option =>
        {
            option.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
            option.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            option.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault;
            option.JsonSerializerOptions.Converters.Add(new FrameLineJsonConverter());
        });
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddCors(opt => opt.AddDefaultPolicy(policy =>
        {
            string[] origins = {
        "https://stack.codewithsaar.net",
        "https://ms.portal.azure.com",
        "https://portal.azure.com"
            };

            policy.WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod();
        }));

        builder.Services.RegisterExceptionBeautifier();
        builder.Services.TryAddScoped<HtmlSectionRender>();
        builder.Services.TryAddScoped<HtmlRender>();

#if DEBUG
foreach (ServiceDescriptor descriptor in builder.Services)
{
    Console.WriteLine("{0}:{1}", descriptor.ServiceType.Name, descriptor.ImplementationType?.Name);
}
#endif

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseHttpsRedirection();

        app.MapHealthChecks("/healthz", new HealthCheckOptions
        {
            AllowCachingResponses = false,
            ResponseWriter = HealthCheckJsonWriter.WriteResponse,
        });

        //Setting the Default Files
        app.UseDefaultFiles(new DefaultFilesOptions()
        {
            DefaultFileNames = new List<string>{
        "index.html"
    },
        });

        app.UseStaticFiles();

        app.UseCors();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}