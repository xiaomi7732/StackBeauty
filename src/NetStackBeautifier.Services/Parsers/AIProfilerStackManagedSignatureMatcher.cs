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
    // Group 1: Full class name
    // Group 2: Method Name
    // Group 3: Parameter list
    // Group 4*: File Path Info
    // Group 5*: Line Number
    // Group 6*: Language
    // 
    // Group 4 and 5 are optional
    private const string MatchExpression = @"^(.*)\.(.*)?\((.*)\)(?:\s*(.*?)\s*([\d]+)\s*(.*))?$";
    private readonly Regex _matcher = new(MatchExpression, RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));

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

        string parameterList = match.Groups[3].Value;
        string[] tokens = parameterList.Split(',', StringSplitOptions.RemoveEmptyEntries);
        List<FrameParameter> methodParameters = new List<FrameParameter>();
        foreach (string parameterDescriptor in tokens)
        {
            string? parameterType = parameterDescriptor.Split(' ', StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
            if (!string.IsNullOrEmpty(parameterType))
            {
                methodParameters.Add(new FrameParameter(parameterType.Trim(), string.Empty));
            }
        }

        FrameFileInfo? frameFileInfo = match.Groups[5].Success ? new FrameFileInfo(match.Groups[4].Value, int.Parse(match.Groups[5].Value)) : null;
        newFrameItem = new FrameItem()
        {
            FullClass = frameClassNameFactory.FromString(match.Groups[1].Value),
            Method = new FrameMethod()
            {
                Name = match.Groups[2].Value,
                Parameters = methodParameters,
            },
            FileInfo = frameFileInfo,
            AssemblySignature = assemblyInfo,
        };

        return true;
    }
}