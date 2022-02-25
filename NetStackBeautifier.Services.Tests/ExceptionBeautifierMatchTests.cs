using System.Linq;
using NetStackBeautifier.Core;
using NetStackBeautifier.Services.StackBeautifiers;
using Xunit;

namespace NetStackBeautifier.Services.Tests;

public class UnitTest1
{
    [Theory]
    [InlineData(@"Unhandled exception. System.InvalidCastException: This is an intended exception
   at Program.<Main>$(String[] args) in D:\Demo\LearnThrow\Program.cs:line 3", true)]
   [InlineData(@"Unhandled exception. System.InvalidCastException: This is an intended exception Program.<Main>$(String[] args) in D:\Demo\LearnThrow\Program.cs:line 3", false)] // No "at ", no match.
    public void ShouldMatch(string input, bool matchOrNot)
    {
        SimpleExceptionStackBeautifier target = new SimpleExceptionStackBeautifier(
            new LineBreaker(), 
            Enumerable.Empty<ILineBeautifier<SimpleExceptionStackBeautifier>>(),
            FrameClassNameFactory.Instance);

        bool actual = target.CanBeautify(input);

        Assert.Equal(matchOrNot, actual);
    }
}