using System.Text.RegularExpressions;
using NetStackBeautifier.Core;
using NetStackBeautifier.Services.Filters;

namespace NetStackBeautifier.Services.StackBeautifiers;
/// <summary>
/// This is an exception stack beautifier that handles exception trace without inner exception.
/// </summary>
internal class SimpleExceptionStackBeautifier : BeautifierBase<SimpleExceptionStackBeautifier>
{
    private readonly FrameClassNameFactory _frameClassNameFactory;

    private const string RegexMatcherExpression = @"Exception.*?at\s";
    private readonly Regex _regex = new Regex(
        RegexMatcherExpression,
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant,
        TimeSpan.FromSeconds(1));

    // For example, match `D:\Demo\LearnThrow\Program.cs and 3` in this following:
    // at Program.<Main>$(String[] args) in D:\Demo\LearnThrow\Program.cs:line 33
    // Group [1]: Full group starting from last in: in D:\Demo\LearnThrow\Program.cs:line 33
    // Group [2]: FilePath: D:\Demo\LearnThrow\Program.cs
    // Group [3]: Line number: 33
    private const string FileInfoRegExpression = @".+ (in (.*):line (\d*))";
    private readonly Regex _fileInfoRegex = new Regex(FileInfoRegExpression, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));

    // Main parts for a line, group 4: full class; group 5: method; group 6: parameters
    // For example, break this: at ABC.Program.<<Main>$>g__TestGenerics|0_0[T](T target, String s) down to:
    // [1] ABC.Program, [2]<<Main>$>g__TestGenerics|0_0[T], [3] T target, String s
    // OR
    // Group 1: full class; group 2: method; group 3: Assembly Info
    // For example: 
    private const string MainPartsRegExpression = @"^\s*at\s+(.*)\.(.*)?\s\((.*)\).*$|^\s*at\s+(.*)\.(.*)?\((.*)\).*$";
    private readonly Regex _mainPartsRegExrepssion = new Regex(MainPartsRegExpression, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));

    // Assembly info with path in it.
    // For example: ServiceProfiler.Web.Stamp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a: C:\a\1\s\src\ServiceProfiler.Web.Stamp\Controllers\HealthCheckApiController.cs:93
    // Will match: 
    // Assembly Info: Group[1]: ServiceProfiler.Web.Stamp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
    // Path: C:\a\1\s\src\ServiceProfiler.Web.Stamp\Controllers\HealthCheckApiController.cs
    // Line: 93
    // On the other hand, it won't match when there is no path info, like: Azure.Identity, Version=1.4.0.0, Culture=neutral, PublicKeyToken=92742159e12e44c8
    private const string AssemblyInfoWithPathInfo = @"^[\s]*(.*?):[\s]*(.*):[\s]*([\d]+)$";
    private readonly Regex _assemblyInfoWithPathMatcher = new Regex(AssemblyInfoWithPathInfo, RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));

    // Simple matcher to split method name and its generic parameters
    // For example: TestGenerics[T,T2]
    // Group[1] method name: TestGenerics
    // Group[2] Type parameter list: T,T2
    private const string MethodNameGenericParameterExp = @"^(.*)\[(.*)\]$";
    private readonly Regex _methodNameTypeParameterMatch = new Regex(MethodNameGenericParameterExp, RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));

    public SimpleExceptionStackBeautifier(
        LineBreaker lineBreaker,
        IEnumerable<ILineBeautifier<SimpleExceptionStackBeautifier>> lineBeautifiers,
        IEnumerable<IFrameFilter<SimpleExceptionStackBeautifier>> filters,
        FrameClassNameFactory frameClassNameFactory)
            : base(lineBreaker, lineBeautifiers, filters)
    {
        _frameClassNameFactory = frameClassNameFactory ?? throw new ArgumentNullException(nameof(frameClassNameFactory));
    }

    public override bool CanBeautify(string input)
    {
        return _regex.IsMatch(input);
    }

    protected override IFrameLine CreateFrameItem(string line)
    {
        if (line.TrimStart().StartsWith("at "))
        {
            return CreateFrame(line);
        }

        return new FrameRawText() { Value = line };
    }

    private FrameItem CreateFrame(string line)
    {
        (FrameFileInfo? frameFileInfo, int fileInfoLength) = GetFileInfo(line);

        // The the rest;
        if (fileInfoLength > 0)
        {
            line = line.Substring(0, line.Length - fileInfoLength - 1).TrimStart();
        }

        Match mainPartsMatch = _mainPartsRegExrepssion.Match(line);
        if (!mainPartsMatch.Success)
        {
            throw new InvalidCastException($"Expected 4 matches for main parts. Original string: {line}, regular expression: {MainPartsRegExpression}");
        }

        (string assemblyInfo, FrameFileInfo? fileInfoInFrameSignature) = ParseAssemblySignature(mainPartsMatch.Groups[3].Value);
        frameFileInfo ??= fileInfoInFrameSignature;

        string methodName = PickSolidStringFromGroup(mainPartsMatch.Groups[2], mainPartsMatch.Groups[5]);
        (methodName, IEnumerable<string> methodTypeParameters) = ParseMethodGenericParameters(methodName);
        FrameItem newFrameItem = new FrameItem()
        {
            FullClass = _frameClassNameFactory.FromString(PickSolidStringFromGroup(mainPartsMatch.Groups[4], mainPartsMatch.Groups[1])),
            Method = new FrameMethod()
            {
                Name = methodName,
                Parameters = ParseParameters(mainPartsMatch.Groups[6].Value),
                GenericParameterTypes = methodTypeParameters,
            },
            FileInfo = frameFileInfo,
            AssemblySignature = assemblyInfo,
        };

        return newFrameItem;
    }

    /// <summary>
    /// Gets a parameter list from a parameter list string. For example:
    /// "T target, String s" to 2 items of frame parameter.
    /// </summary>
    private IEnumerable<FrameParameter> ParseParameters(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            yield break;
        }

        string[] pairs = input.Split(',', StringSplitOptions.RemoveEmptyEntries);
        foreach (string pair in pairs)
        {
            string[] typeAndNameTokens = pair.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (typeAndNameTokens.Length != 2)
            {
                throw new InvalidCastException($"Unexpected parameter pair: {pair}, input string: {input}");
            }
            yield return new FrameParameter(typeAndNameTokens[0], typeAndNameTokens[1]);
        }
    }

    private (FrameFileInfo?, int) GetFileInfo(string line)
    {
        Match match = _fileInfoRegex.Match(line);
        if (match is null || !match.Success)
        {
            return (null, 0);
        }

        if (match.Groups.Count != 4)
        {
            throw new InvalidCastException($"Expecting 3 groups in a match. Original string: {line}, Regex: {FileInfoRegExpression}");
        }

        int length = match.Groups[1].Length;        // Full match
        string filePath = match.Groups[2].Value;    // Full path to the file.
        int lineNumber = int.Parse(match.Groups[3].Value);  // Line number.

        FrameFileInfo result = new FrameFileInfo(filePath, lineNumber);
        return (result, length);
    }

    private (string, FrameFileInfo? fileInfo) ParseAssemblySignature(string assemblySignature)
    {
        if (string.IsNullOrEmpty(assemblySignature))
        {
            return (string.Empty, null);
        }

        Match match = _assemblyInfoWithPathMatcher.Match(assemblySignature);
        if (match.Success)
        {
            return (match.Groups[1].Value, new FrameFileInfo(match.Groups[2].Value, int.Parse(match.Groups[3].Value)));
        }

        // Otherwise
        return (assemblySignature, null);
    }

    private string PickSolidStringFromGroup(params Group[] groups)
    {
        return groups.First(g => !string.IsNullOrEmpty(g.Value)).Value;
    }

    private (string, IEnumerable<string>) ParseMethodGenericParameters(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return (string.Empty, Enumerable.Empty<string>());
        }

        Match match = _methodNameTypeParameterMatch.Match(input);
        if (match.Success)
        {
            return (match.Groups[1].Value, match.Groups[2].Value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        }

        return (input, Enumerable.Empty<string>());
    }
}