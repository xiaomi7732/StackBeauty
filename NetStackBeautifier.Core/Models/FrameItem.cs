namespace NetStackBeautifier.Core;
public record FrameItem : IFrameLine
{
    public FrameFullClass? FullClass { get; init; }

    public FrameMethod? Method { get; init; }

    public FrameFileInfo? FileInfo { get; init; }

    public Guid Id { get; } = Guid.NewGuid();

    public string? AssemblySignature { get; init; }

    public IDictionary<string, string> Tags { get; init; } = new Dictionary<string, string>();
}