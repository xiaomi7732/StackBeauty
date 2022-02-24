namespace NetStackBeautifier.Core;
public record FrameItem : IFrameLine
{
    public FrameFullClass? FullClass { get; init; }

    public FrameMethod? Method { get; init; }

    public FrameFileInfo? FileInfo { get; init; }

    public Guid Id { get; } = Guid.NewGuid();

    public List<Guid> Children { get; } = new List<Guid>();
}