namespace NetStackBeautifier.Core
{
    public record FrameMethod
    {
        public string? Name { get; init; }

        /// <summary>
        /// Gets or initializes the parameter list for the method.
        /// </summary>
        public IEnumerable<FrameParameter> Parameters { get; init; } = Enumerable.Empty<FrameParameter>();

        /// <summary>
        /// Generic types.
        /// </summary>
        /// <typeparam name="string"></typeparam>
        public IEnumerable<string> GenericParameterTypes{get; init;} = Enumerable.Empty<string>();

        /// <summary>
        /// Generic types.
        /// </summary>
        /// <typeparam name="string"></typeparam>
        public IEnumerable<string> RawGenericParameterTypes{get; init;} = Enumerable.Empty<string>();
    }
}