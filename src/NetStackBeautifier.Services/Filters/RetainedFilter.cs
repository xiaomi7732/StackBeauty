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
        frameLine.Tags.TryAdd(KnownTagKeys.FullLine, rawRequest.Value.Trim());
        frameLine.Tags.Add(KnownTagKeys.StartingIndex, rawRequest.Index.ToString());
    }
}