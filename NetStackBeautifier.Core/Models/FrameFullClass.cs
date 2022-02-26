namespace NetStackBeautifier.Core;
/// <summary>
/// A class represnets a full class, including namespace. 
/// </summary>
public record FrameFullClass
{
    public IEnumerable<string> NameSections { get; init; } = Enumerable.Empty<string>();
    public IEnumerable<string> GenericParameterTypes { get; init; } = Enumerable.Empty<string>();

    /// <summary>
    /// Gets the full class name.
    /// </summary>
    /// <value></value>
    public string? FullClassNameOrDefault =>
        NameSections switch
        {
            null => null,
            _ => string.Join('.', NameSections)
        };

    /// <summary>
    /// Gets the last section in the namespace
    /// </summary>
    /// <returns></returns>
    public string? ShortClassNameOrDefault
    {
        get
        {
            string? result = NameSections?.LastOrDefault();
            if (string.IsNullOrEmpty(result))
            {
                return null;
            }

            // Special: d__9, d__?
            if (result.StartsWith("d__", StringComparison.Ordinal))
            {
                string numberPart = result.Substring("d__".Length);
                int sectionNum = NameSections.NullAsEmpty().Count();
                if (sectionNum > 1)
                {
                    return string.Join('.', NameSections.Skip(sectionNum - 2));
                }
            }

            return result;
        }
    }
}