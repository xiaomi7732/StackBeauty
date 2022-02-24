namespace NetStackBeautifier.Core;

/// <summary>
/// A factory to create frame namespace object.
/// </summary>
public sealed class FrameClassNameFactory
{
    private FrameClassNameFactory()
    {
    }
    public static FrameClassNameFactory Instance { get; } = new FrameClassNameFactory();

    /// <summary>
    /// A factory method to create namespace from string.
    /// </summary>
    /// <param name="namespaceString"></param>
    /// <returns></returns>
    public FrameFullClass FromString(string namespaceString)
    {
        string[] namespaceTokens = namespaceString.Split('.', StringSplitOptions.RemoveEmptyEntries);
        return new FrameFullClass()
        {
            NameSections = namespaceTokens,
        };
    }

    /// <summary>
    /// A factory method to create namespace from string, and a list of generic types
    /// </summary>
    /// <param name="namespaceString"></param>
    /// <returns></returns>
    public FrameFullClass FromString(string namespaceString, params string[] genericTypes)
    {
        string[] namespaceTokens = namespaceString.Split('.', StringSplitOptions.RemoveEmptyEntries);

        return new FrameFullClass()
        {
            NameSections = namespaceTokens,
            GenericParameterTypes = genericTypes,
        };
    }
}