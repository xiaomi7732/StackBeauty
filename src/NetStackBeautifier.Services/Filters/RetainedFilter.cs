namespace NetStackBeautifier.Services.Filters;

internal class RetainedFilter<T> : IPreFilter<T>
    where T : IBeautifier
{
    public void Filter(IFrameLine frameLine, RawRequest rawRequest)
    {
        if (frameLine is null || rawRequest is null)
        {
            return;
        }
        frameLine.Tags.TryAdd(KnownTagKey.FullLine, rawRequest.Value.Trim());
        frameLine.Tags.Add(KnownTagKey.StartingIndex, rawRequest.Index.ToString());
    }
}