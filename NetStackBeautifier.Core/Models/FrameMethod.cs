namespace NetStackBeautifier.Core
{
    public record FrameMethod
    {
        public string? Name { get; init; }

        public IEnumerable<FrameParameter> Parameters { get; init; } = Enumerable.Empty<FrameParameter>();
    }
}