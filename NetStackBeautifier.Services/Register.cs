using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NetStackBeautifier.Core;
using NetStackBeautifier.Services.ExceptionBeautifiers;
using NetStackBeautifier.Services.LineBeautifiers;

namespace NetStackBeautifier.Services
{
    public static class Register
    {
        public static IServiceCollection RegisterExceptionBeautifier(this IServiceCollection services)
        {
            services.TryAddScoped<LineBreaker>();
            services.TryAddSingleton<FrameClassNameFactory>(p => FrameClassNameFactory.Instance);
            services.AddScoped<IBeautifier, SimpleExceptionStackBeautifier>();

            services.AddScoped(typeof(ILineBeautifier<>), typeof(GenericMethodBeautifier<>));
            services.AddScoped<IBeautifier, DumbBeautifier>();
            services.TryAddScoped<IBeautifierService, BeautifierService>();

            return services;
        }
    }
}