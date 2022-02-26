namespace NetStackBeautifier
{
    public interface IFrameLine
    {
        Guid Id { get; }
        IDictionary<string, string> Tags { get; }
    }
}