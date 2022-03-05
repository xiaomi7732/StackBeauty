namespace NetStackBeautifier.Core;
public record FrameRawText : IFrameLine
{
    public string Value { get; init; } = string.Empty;

    public Guid Id { get; init; } = Guid.NewGuid();

    public IDictionary<string, string> Tags { get; init; } = new Dictionary<string, string>();
}