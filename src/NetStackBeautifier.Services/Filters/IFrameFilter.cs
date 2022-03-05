namespace NetStackBeautifier.Services.Filters;
public interface IFrameFilter<T>
    where T: IBeautifier
{
    /// <summary>
    /// Adding tags to an IFrameLine.
    /// </summary>
    void Filter(IFrameLine frameline);
}