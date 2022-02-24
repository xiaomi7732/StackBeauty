using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NetStackBeautifier.Core;

namespace NetStackBeautifier.Services
{
    public static class Register
    {
        public static IServiceCollection RegisterExceptionBeautifier(this IServiceCollection services)
        {
            services.TryAddScoped<LineBreaker>();
            services.TryAddSingleton<FrameClassNameFactory>(p => FrameClassNameFactory.Instance);
            services.AddScoped<IBeautifier, SimpleExceptionStackBeautifier>();
            services.AddScoped<IBeautifier, DumbBeautifier>();
            services.TryAddScoped<IBeautifierService, BeautifierService>();

            return services;
        }
    }
}