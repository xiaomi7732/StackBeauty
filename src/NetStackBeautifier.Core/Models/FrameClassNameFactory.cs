using System.Collections.ObjectModel;
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

    // Other typical cases
    //    at Microsoft.ServiceProfiler.Internal.Service.Authority.CentralStorageAuthority+<>c__DisplayClass2_1+<<-ctor>b__1>d.MoveNext (Microsoft.ServiceProfiler.Internal.Service, Version=2.6.2107.1202, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a)
    //    at Microsoft.Extensions.Caching.Memory.CacheExtensions+<GetOrCreateAsync>d__9`1.MoveNext (Microsoft.Extensions.Caching.Abstractions, Version=3.1.2.0, Culture=neutral, PublicKeyToken=adb9793829ddae60)
    private const string SectionMatcherExpression = @"[\.]?(?<ng>[^\.\s]+)?\[(?<g>.*)\]|[\.]?(?<n>[^\.\s]+)";
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

    private IEnumerable<string> GetSectionNames(MatchCollection matches)
    {
        return ReassembleInheritedClass(BreakDown(matches).ToArray());
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

    private IEnumerable<string> BreakDown(MatchCollection matches)
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

    /// <summary>
    /// This is a patch implementation when the generic forms is broken by . spliter. For example: 
    /// NetStackBeautifier.Services.BeautifierBase<NetStackBeautifier.Services.StackBeautifiers.SimpleExceptionStackBeautifier>.BeautifyAsync() will be broken down to:
    /// NetStackBeautifier, Services, BeautifierBase<NetStackBeautifier, Services, StackBeautifiers, SimpleExceptionStackBeautifier>, ...
    /// Notice that the portion by <> has been broken down unintentionally. This patch method merges it back.
    /// This is not ideal, but at least make it a bit easier for reading or post processing.
    /// </summary>
    internal static IEnumerable<string> ReassembleInheritedClass(string[] tokens)
    {

        int total = tokens.Length;
        int start = total;
        int end = 0;
        int hit = 0;
        bool everHit = false;
        bool fullMatch = false;
        for (int i = 0; i < total; i++)
        {
            if (tokens[i].Contains('<'))
            {
                hit++;
                everHit = true;
                start = i;
            }

            if (tokens[i].Contains('>') && everHit)
            {
                hit--;

                if (hit == 0)
                {
                    end = i;
                    fullMatch = true;
                    break;
                }
            }
        }

        // Invalid match
        if(!fullMatch)
        {
            foreach(string token in tokens)
            {
                yield return token;
            }
            yield break;
        }

        // Valid match
        for (int i = 0; i < start; i++)
        {
            yield return tokens[i];
        }
        //Valid match but never hit ... this shouldn't happen.
        if (!everHit)
        {
            yield break;
        }
        
        yield return string.Join('.', tokens, startIndex: start, count: end - start + 1);

        for (int i = end + 1; i < total; i++)
        {
            yield return tokens[i];
        }
    }
}