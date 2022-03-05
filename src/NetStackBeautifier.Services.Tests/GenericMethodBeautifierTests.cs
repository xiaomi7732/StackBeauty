using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NetStackBeautifier.Core;
using NetStackBeautifier.Services.LineBeautifiers;
using NetStackBeautifier.Services.StackBeautifiers;
using Xunit;

namespace NetStackBeautifier.Services.Tests;

public class GenericMethodBeautifierTests
{
    [Theory]
    [InlineData(@"TestGenerics[T,T2]", @"TestGenerics<T, T2>")]
    public async Task TestPositiveBeautify(string input, string expected)
    {
        Mock<ILogger<GenericMethodBeautifier<SimpleExceptionStackBeautifier>>> loggerMock = new();
        GenericMethodBeautifier<SimpleExceptionStackBeautifier> target = new GenericMethodBeautifier<SimpleExceptionStackBeautifier>(loggerMock.Object);

        FrameItem frameItem = new FrameItem()
        {
            Method = new FrameMethod()
            {
                Name = input,
            }
        };

        frameItem = await target.BeautifyAsync(frameItem, cancellationToken: default);

        Assert.NotNull(frameItem.Method);
        Assert.Equal(expected, frameItem.Method!.Name);
    }
}