namespace NetStackBeautifier.Core;
public record FrameRawText : IFrameLine
{
    public string Value { get; init; } = string.Empty;

    public Guid Id { get; init; } = Guid.NewGuid();

    public List<Guid> Children { get; } = new List<Guid>();
}