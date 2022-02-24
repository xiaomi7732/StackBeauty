using System.Text.RegularExpressions;
using NetStackBeautifier.Core;

namespace NetStackBeautifier.Services
{
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

        // Main parts for a line, group 1: full class; group 2: method; group 3: parameters
        // For example, break this: at ABC.Program.<<Main>$>g__TestGenerics|0_0[T](T target, String s) down to:
        // [1] ABC.Program, [2]<<Main>$>g__TestGenerics|0_0[T], [3] T target, String s
        private const string MainPartsRegExpression = @"^.*at\s*(.*)\.(.*)?\((.*)\).*$";
        private readonly Regex _mainPartsRegExrepssion = new Regex(MainPartsRegExpression, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));

        public SimpleExceptionStackBeautifier(
            LineBreaker lineBreaker,
            IEnumerable<ILineBeautifier<SimpleExceptionStackBeautifier>> lineBeautifiers,
            FrameClassNameFactory frameClassNameFactory)
                : base(lineBreaker, lineBeautifiers)
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
            line = line.Substring(0, line.Length - fileInfoLength - 1).TrimStart();
            // TODO: Keep processing
            Match mainPartsMatch = _mainPartsRegExrepssion.Match(line);
            if (!mainPartsMatch.Success || mainPartsMatch.Groups.Count != 4)
            {
                throw new InvalidCastException($"Expected 4 matches for main parts. Original string: {line}, regular expression: {MainPartsRegExpression}");
            }

            FrameItem newFrameItem = new FrameItem()
            {
                FullClass = _frameClassNameFactory.FromString(mainPartsMatch.Groups[1].Value),
                Method = new FrameMethod()
                {
                    Name = mainPartsMatch.Groups[2].Value,
                    Parameters = ParseParameters(mainPartsMatch.Groups[3].Value),
                },
                FileInfo = frameFileInfo,
            };

            return newFrameItem;
        }

        /// <summary>
        /// Gets a parameter list from a parameter list string. For example:
        /// "T target, String s" to 2 items of frame parameter.
        /// </summary>
        private IEnumerable<FrameParameter> ParseParameters(string input)
        {
            string[] pairs = input.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (string pair in pairs)
            {
                string[] typeAndNameTokens = pair.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (typeAndNameTokens.Length != 2)
                {
                    throw new InvalidCastException($"Unexpected parameter pair: {pair}");
                }
                yield return new FrameParameter(typeAndNameTokens[0], typeAndNameTokens[1]);
            }
        }

        private (FrameFileInfo?, int) GetFileInfo(string line)
        {
            Match match = _fileInfoRegex.Match(line);
            if (match is null)
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
    }
}