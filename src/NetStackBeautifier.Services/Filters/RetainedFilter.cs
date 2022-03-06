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
        frameLine.Tags.TryAdd("fullLine", fullLine);
        frameLine.Tags.Add("startingIndex", startingIndex.ToString());
    }
}