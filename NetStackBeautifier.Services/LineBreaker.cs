namespace NetStackBeautifier.Services
{
    public class LineBreaker
    {
        public string[] BreakIntoLines(string input) => input.Split('\n', StringSplitOptions.RemoveEmptyEntries);
    }
}