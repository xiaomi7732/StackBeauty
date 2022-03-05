namespace NetStackBeautifier.Services.Filters;

internal class RetainedFilter<T> : IPreFilter<T>
    where T : IBeautifier
{
    public void Filter(IFrameLine frameLine, string fullLine, int startingIndex)
    {
        if (frameLine == null)
        {
            return;
        }
        frameLine.Tags.TryAdd("FullLine", fullLine);
        frameLine.Tags.Add("StartingIndex", startingIndex.ToString());
    }
}