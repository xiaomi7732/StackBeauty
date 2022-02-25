using System.Text.RegularExpressions;

namespace NetStackBeautifier.Core;

/// <summary>
/// A factory to create frame namespace object.
/// </summary>
public sealed class FrameClassNameFactory
{
    // Matches either Generic or non-generic sections.
    // For example. When there is no generic, the first group is the name; When there is generic, the group[2] contains types for generics, separated by comma.
    // Microsoft.Commerce.Collections.KeyedCollection`2[System.__Canon,System.__Canon] =>
    // Match[1], Group["n"]: Microsoft
    // Match[2], Group["n"]: Commerce
    // Match[3], Group["n"]: Collections
    // Match[4], Group["ng"]: KeyedCollection`2, Group["g"]: System.__Canon,System.__Canon
    private const string SectionMatcherExpression = @"[\.]?(?<ng>[\w|`]+)\[(?<g>.*)\]|[\.]?(?<n>\w+)";
    private static readonly Regex sectionMatcher = new Regex(SectionMatcherExpression, RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));

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
        MatchCollection matches = sectionMatcher.Matches(namespaceString);

        return new FrameFullClass()
        {
            NameSections = GetSectionNames(matches),
            GenericParameterTypes = GetGenericParameterTypes(matches.LastOrDefault()),
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

    private IEnumerable<string> GetSectionNames(MatchCollection matches)
    {
        foreach (Match match in matches)
        {
            foreach (Group group in match.Groups)
            {
                if (!string.IsNullOrEmpty(group.Value) &&
                    (string.Equals(group.Name, "n", StringComparison.OrdinalIgnoreCase) || string.Equals(group.Name, "ng", StringComparison.OrdinalIgnoreCase)))
                {
                    yield return group.Value;
                }
            }
        }
    }

    private IEnumerable<string> GetGenericParameterTypes(Match? match)
    {
        if (match is null)
        {
            yield break;
        }

        foreach (Group group in match.Groups)
        {
            if (!string.IsNullOrEmpty(group.Name) && string.Equals(group.Name, "g", StringComparison.OrdinalIgnoreCase))
            {
                foreach (string item in group.Value.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    yield return item;
                }
            }
        }
    }
}