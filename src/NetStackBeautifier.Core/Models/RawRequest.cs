namespace NetStackBeautifier.Services
{
    public class RawRequest
    {
        public RawRequest(string value, int index)
        {
            Value = value;
            Index = index;
        }
        public int Index { get; }
        public string Value { get; }
    }
}
