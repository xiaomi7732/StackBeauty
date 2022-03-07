namespace NetStackBeautifier.Services.Filters;
public interface IPreFilter<T>
    where T: IBeautifier
{
    /// <summary>
    /// Adding tags to an IFrameLine with pre-processed information
    /// </summary>
    void Filter(IFrameLine frameline, RawRequest rawRequest);
}