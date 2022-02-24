namespace NetStackBeautifier
{
    public interface IFrameLine
    {
        Guid Id { get; }
        List<Guid> Children { get; }
    }
}