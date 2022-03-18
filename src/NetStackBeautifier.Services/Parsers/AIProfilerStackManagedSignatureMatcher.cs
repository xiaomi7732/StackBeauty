using System.Text.RegularExpressions;
using NetStackBeautifier.Core;

namespace NetStackBeautifier.Services.StackBeautifiers;

/// <summary>
/// Builds a FrameItem from a line of managed code.
/// </summary>
public class AIProfilerStackManagedSignatureMatcher
{
    // Matches managed code signature. For example:
    // Microsoft.Azure.ChangeAnalysis.Authorization.AzureResourceManagerAuthorization.AcquireAuthorizationTokenAsync(System.Uri authority, bool forceRefreshToken, System.Threading.CancellationToken cancellationToken) Line 42 C#
    // Group 1: Full class name.MethodName  ==> Microsoft.Azure.ChangeAnalysis.Authorization.AzureResourceManagerAuthorization.AcquireAuthorizationTokenAsync
    // Group 2: Parameter list  ==> System.Uri authority, bool forceRefreshToken, System.Threading.CancellationToken cancellationToken
    // Group 3: Rest* ==> Line 42 C#
    private const string MatchExpression = @"^(.*?)\((.*?)\)(.*)";
    private readonly Regex _matcher = new(MatchExpression, RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));
    private readonly AIProfilerStackFileInfoMatcher _fileInfoMatcher;

    public AIProfilerStackManagedSignatureMatcher(AIProfilerStackFileInfoMatcher fileInfoMatcher)
    {
        _fileInfoMatcher = fileInfoMatcher ?? throw new ArgumentNullException(nameof(fileInfoMatcher));
    }

    /// <summary>
    /// Creates a FrameItem when matches. Returns true. Otherwise, output FrameItem be null and returns false.
    /// </summary>
    public bool TryCreate(
        string content,
        string assemblyInfo,
        FrameClassNameFactory frameClassNameFactory,
        out FrameItem? newFrameItem)
    {
        if (string.IsNullOrEmpty(content))
        {
            throw new ArgumentException($"'{nameof(content)}' cannot be null or empty.", nameof(content));
        }
        newFrameItem = null;
        Match match = _matcher.Match(content);

        if (!match.Success)
        {
            return false;
        }

        // Decide method / full class name
        string[] classMethodTokens = match.Groups[1].Value.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        string fullClass = string.Empty;
        string methodName = string.Empty;
        if (classMethodTokens.Length == 1)
        {
            methodName = classMethodTokens[0];
        }
        else
        {
            fullClass = string.Join('.', classMethodTokens.SkipLast(1));
            methodName = classMethodTokens[classMethodTokens.Length - 1];
        }

        string parameterList = match.Groups[2].Success ? match.Groups[2].Value : string.Empty;
        IEnumerable<FrameParameter> methodParameters = CreateFrameParameters(parameterList);

        // FrameFileInfo? frameFileInfo = match.Groups[5].Success ? new FrameFileInfo(match.Groups[4].Value, int.Parse(match.Groups[5].Value)) : null;
        newFrameItem = new FrameItem()
        {
            FullClass = frameClassNameFactory.FromString(fullClass),
            Method = new FrameMethod()
            {
                Name = methodName,
                Parameters = methodParameters,
            },
            FileInfo = CreateFrameFileInfo(match.Groups[3].Success ? match.Groups[3].Value : string.Empty),
            AssemblySignature = assemblyInfo,
        };

        return true;
    }

    private IEnumerable<FrameParameter> CreateFrameParameters(string parameterList)
    {
        if (string.IsNullOrEmpty(parameterList))
        {
            return Enumerable.Empty<FrameParameter>();
        }

        List<FrameParameter> methodParameters = new List<FrameParameter>();
        string[] tokens = parameterList.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        // Special case: some parameter will be marked by 'class '
        for (int i = 0; i < tokens.Length; i++)
        {
            if (tokens[i].StartsWith("class ", StringComparison.OrdinalIgnoreCase))
            {
                tokens[i] = tokens[i].Substring("class ".Length);
            }
        }

        foreach (string parameterDescriptor in tokens)
        {
            string[] parameterSet = parameterDescriptor.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            string type = string.Empty;
            string name = string.Empty;
            if (parameterSet.Length == 1)
            {
                type = parameterSet[0];
            }
            else if (parameterSet.Length == 2)
            {
                type = parameterSet[0];
                name = parameterSet[1];
            }

            if (!string.IsNullOrEmpty(type) || !string.IsNullOrEmpty(name))
            {
                methodParameters.Add(new FrameParameter(type, name));
            }
        }

        return methodParameters;
    }

    private FrameFileInfo? CreateFrameFileInfo(string content)
    {
        if (string.IsNullOrEmpty(content))
        {
            return null;
        }

        _fileInfoMatcher.TryCreate(content, out FrameFileInfo? result);
        return result;
    }
}