namespace NetStackBeautifier.Core;
/// <summary>
/// A class represnets a full class, including namespace. 
/// </summary>
public record FrameFullClass
{
    public IEnumerable<string>? NameSections { get; init; }

    /// <summary>
    /// Gets the full class name.
    /// </summary>
    /// <value></value>
    public string? FullClassNameOrDefault =>
        NameSections switch {
            null => null,
            _ => string.Join('.', NameSections)
        };

    /// <summary>
    /// Gets the last section in the namespace
    /// </summary>
    /// <returns></returns>
    public string? ShortClassNameOrDefault =>
        NameSections?.LastOrDefault();
}