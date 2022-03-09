using System.Text.RegularExpressions;
using NetStackBeautifier.Core;
using NetStackBeautifier.Services.Filters;

namespace NetStackBeautifier.Services.StackBeautifiers;

internal class AIProfilerStackBeautifier : BeautifierBase<AIProfilerStackBeautifier>
{
    private readonly LineBreaker _lineBreaker;
    private readonly FrameClassNameFactory _frameClassNameFactory;

    // AI Profiler stack beautifier
    private const string CanBeautifyMatcher = @".*?!.*?\(.*\)";
    private static readonly Regex _canBeautifyMatcher = new Regex(CanBeautifyMatcher, RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));

    // Break a line into 2 parts, get the entry and the rest. For example:
    // clr!SlowAllocateString(!!0x1) will be break into:
    // Group[1]:Entry:clr, Group[2]:Rest:SlowAllocateString(!!0x1).
    private const string EntryInfoMatcherExp = @"^(.*?)!(.*)$";
    private static readonly Regex _entryInfoMatcher = new Regex(EntryInfoMatcherExp, RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));

    // Native method matcher
    // WKS::GCHeap::Alloc
    // Group[1]-Class-WKS::GCHeap
    // Group[2]-Alloc
    private const string CppMatcherExp = @"^(.*)::(.*)$";
    private static readonly Regex _cppMatcher = new Regex(CppMatcherExp, RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));

    // For special case that only a pure string, no special chars like . or () or anything like that.
    private const string SingleStringExp = @"^[\w]+$";
    private static readonly Regex _singleStringMatcher = new Regex(SingleStringExp, RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));


    // Matches managed class/method signature
    // For example:
    // DiagService.Controllers.MemoryController.StringSubstring(int32)
    // Group[1]-FullClass-DiagService.Controllers.MemoryController
    // Group[2]-Method-StringSubstring
    // Group[3]-Parameter list.
    private const string ManagedSignatureMatcher = @"^(.*)\.(.*)?\((.*)\)$";
    private static readonly Regex _managedSigMatcher = new Regex(ManagedSignatureMatcher, RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));

    public AIProfilerStackBeautifier(
        LineBreaker lineBreaker,
        IEnumerable<ILineBeautifier<AIProfilerStackBeautifier>> lineBeautifiers,
        IEnumerable<IFrameFilter<AIProfilerStackBeautifier>> filters,
        IEnumerable<IPreFilter<AIProfilerStackBeautifier>> preFilters,
        FrameClassNameFactory frameClassNameFactory)
        : base(lineBreaker, lineBeautifiers, filters, preFilters)
    {
        _lineBreaker = lineBreaker ?? throw new ArgumentNullException(nameof(lineBreaker));
        _frameClassNameFactory = frameClassNameFactory ?? throw new ArgumentNullException(nameof(frameClassNameFactory));
    }

    public override bool CanBeautify(string input)
    {
        if (_canBeautifyMatcher.Match(input).Success)
        {
            return true;
        }

        foreach (string line in _lineBreaker.BreakIntoLines(input))
        {
            // If there's any of these, it is AI Stack
            if (line.StartsWith("anonymously hosted dynamicmethods assembly!", StringComparison.OrdinalIgnoreCase)
                || line.StartsWith("clr!", StringComparison.OrdinalIgnoreCase)
                || line.StartsWith("mscorlib.ni!", StringComparison.OrdinalIgnoreCase)
                || line.StartsWith("mscorlib!", StringComparison.OrdinalIgnoreCase)
                || line.StartsWith("newtonsoft.json!", StringComparison.OrdinalIgnoreCase)
                || line.StartsWith("system.core.ni!", StringComparison.OrdinalIgnoreCase)
                || line.StartsWith("system.web.http!", StringComparison.OrdinalIgnoreCase)
            )
            {
                return true;
            }
        }

        return false;
    }

    protected override IFrameLine CreateFrameItem(string line)
    {
        Match entryMatch = _entryInfoMatcher.Match(line);
        if (!entryMatch.Success)
        {
            return CreateRawLine(line);
        }

        string assemblyInfo = entryMatch.Groups[1].Value;
        string rest = entryMatch.Groups[2].Value.TrimEnd('\r');

        // Special case:
        if (rest.Length == 1 && string.Equals(rest, "?"))
        {
            return CreateRawLine($"Symbol resolution failure for assembly: {assemblyInfo}");
        }

        // Native
        Match cppSigMatch = _cppMatcher.Match(rest);
        if (cppSigMatch.Success)
        {
            return new FrameItem()
            {
                FullClass = _frameClassNameFactory.FromString(cppSigMatch.Groups[1].Value.Replace("::", ".")),
                Method = new FrameMethod() { Name = cppSigMatch.Groups[2].Value },
                AssemblySignature = assemblyInfo,
            };
        }

        // Special case - the rest is a single string
        Match singleStringMatch = _singleStringMatcher.Match(rest);
        if (singleStringMatch.Success)
        {
            return new FrameItem()
            {
                Method = new FrameMethod() { Name = rest },
                AssemblySignature = assemblyInfo,
            };
        }

        // Managed
        Match managedSigMatch = _managedSigMatcher.Match(rest);
        if (managedSigMatch.Success)
        {
            string parameterList = managedSigMatch.Groups[3].Value;
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

            return new FrameItem()
            {
                FullClass = _frameClassNameFactory.FromString(managedSigMatch.Groups[1].Value),
                Method = new FrameMethod()
                {
                    Name = managedSigMatch.Groups[2].Value,
                    Parameters = methodParameters,
                },
                AssemblySignature = assemblyInfo,
            };
        }

        // Fallback
        return CreateRawLine(line);
    }

    private IFrameLine CreateRawLine(string value)
    {
        return new FrameRawText()
        {
            Value = value,
        };
    }
}