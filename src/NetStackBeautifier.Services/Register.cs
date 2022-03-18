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
            // Register fundamentals
            services.TryAddScoped<LineBreaker>();
            services.TryAddScoped<AzureProfilerStackLineCreator>();
            services.TryAddScoped<AIProfilerStackManagedSignatureMatcher>();
            services.TryAddSingleton<FrameClassNameFactory>(p => FrameClassNameFactory.Instance);
            services.TryAddScoped<IBeautifierService, BeautifierService>();

            // Register Parsers
            services.TryAddEnumerable(ServiceDescriptor.Scoped<IBeautifier, AIProfilerStackBeautifier>());
            services.TryAddEnumerable(ServiceDescriptor.Scoped<IBeautifier, SimpleExceptionStackBeautifier>());
            services.TryAddEnumerable(ServiceDescriptor.Scoped<IBeautifier, AzureProfilerStackBeautifier>());

            // Register Beautifiers
            services.TryAddEnumerable(ServiceDescriptor.Scoped(typeof(ILineBeautifier<>), typeof(MethodNameNewLineRemover<>)));
            services.TryAddEnumerable(ServiceDescriptor.Scoped<ILineBeautifier<AzureProfilerStackBeautifier>, ClassNameTickNumberRemover<AzureProfilerStackBeautifier>>());
            services.TryAddEnumerable(ServiceDescriptor.Scoped(typeof(ILineBeautifier<>), typeof(GenericMethodBeautifier<>)));
            services.TryAddEnumerable(ServiceDescriptor.Scoped(typeof(ILineBeautifier<>), typeof(LambdaMethodBeautifier<>)));
            services.TryAddEnumerable(ServiceDescriptor.Scoped(typeof(ILineBeautifier<>), typeof(ClassGenericTypeSystemCanonicalBeautifier<>)));
            services.TryAddEnumerable(ServiceDescriptor.Scoped(typeof(ILineBeautifier<>), typeof(Int32ParameterFiller<>)));
            services.TryAddEnumerable(ServiceDescriptor.Scoped(typeof(ILineBeautifier<>), typeof(MoveNextBeautifier<>)));

            // Register Filters
            services.TryAddEnumerable(ServiceDescriptor.Scoped(typeof(IPreFilter<>), typeof(RetainedFilter<>)));
            services.TryAddEnumerable(ServiceDescriptor.Scoped(typeof(IFrameFilter<>), typeof(NoiseFilter<>)));
            services.TryAddEnumerable(ServiceDescriptor.Scoped<IFrameFilter<SimpleExceptionStackBeautifier>, DICannotInstantiateTagger>());
            services.TryAddEnumerable(ServiceDescriptor.Scoped<IFrameFilter<SimpleExceptionStackBeautifier>, DICannotFindDepTagger>());

            return services;
        }
    }
}