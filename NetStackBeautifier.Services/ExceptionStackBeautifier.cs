using NetStackBeautifier.Core;

namespace NetStackBeautifier.Services
{
    internal class ExceptionStackBeautifier : BeautifierBase<ExceptionStackBeautifier>
    {
        public ExceptionStackBeautifier(
            LineBreaker lineBreaker,
            IEnumerable<ILineBeautifier<ExceptionStackBeautifier>> lineBeautifiers)
                : base(lineBreaker, lineBeautifiers)
        {
        }

        public override bool CanBeautify(string input)
        {
            throw new NotImplementedException();
        }

        protected override FrameItem CreateFrameItem(string line)
        {
            throw new NotImplementedException();
        }
    }
}