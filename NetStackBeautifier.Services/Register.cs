using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NetStackBeautifier.Core;
using NetStackBeautifier.Services.Filters;
using NetStackBeautifier.Services.LineBeautifiers;
using NetStackBeautifier.Services.StackBeautifiers;

namespace NetStackBeautifier.Services
{
    public static class Register
    {
        public static IServiceCollection RegisterExceptionBeautifier(this IServiceCollection services)
        {
            services.TryAddScoped<LineBreaker>();
            services.TryAddSingleton<FrameClassNameFactory>(p => FrameClassNameFactory.Instance);
            services.AddScoped<IBeautifier, AIProfilerStackBeautifier>();
            services.AddScoped<IBeautifier, SimpleExceptionStackBeautifier>();
            services.AddScoped<IBeautifier, AzureProfilerStackBeautifier>();

            services.AddScoped(typeof(ILineBeautifier<>), typeof(MethodNameNewLineRemover<>));
            services.AddScoped<ILineBeautifier<AzureProfilerStackBeautifier>, ClassNameTickNumberRemover<AzureProfilerStackBeautifier>>();
            services.AddScoped(typeof(ILineBeautifier<>), typeof(GenericMethodBeautifier<>));
            services.AddScoped(typeof(ILineBeautifier<>), typeof(ClassGenericTypeSystemCanonicalBeautifier<>));
            services.AddScoped(typeof(ILineBeautifier<>), typeof(Int32ParameterFiller<>));
            services.AddScoped(typeof(ILineBeautifier<>),typeof(MoveNextBeautifier<>));
            services.AddScoped<IBeautifier, DumbBeautifier>();
            services.TryAddScoped<IBeautifierService, BeautifierService>();

            services.TryAddScoped(typeof(IFrameFilter<>), typeof(NoiseFilter<>));
            return services;
        }
    }
}