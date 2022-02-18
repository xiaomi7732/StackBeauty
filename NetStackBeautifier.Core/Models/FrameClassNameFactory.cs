namespace NetStackBeautifier.Core;

/// <summary>
/// A factory to create frame namespace object.
/// </summary>
public sealed class FrameNamespaceFactory
{
    private FrameNamespaceFactory()
    {
    }
    public static FrameNamespaceFactory Instance { get; } = new FrameNamespaceFactory();

    /// <summary>
    /// A factory method to create namespace from string.
    /// </summary>
    /// <param name="namespaceString"></param>
    /// <returns></returns>
    public FrameFullClass FromString(string namespaceString)
    {
        string[] namespaceTokens = namespaceString.Split(',', StringSplitOptions.RemoveEmptyEntries);
        return new FrameFullClass()
        {
            NameSections = namespaceTokens,
        };
    }
}