using FluentAssertions;
using Vexora.Application.NLU.Parsing.Number;
using Xunit;

namespace Vexora.Application.Test.NLU.Number;

public sealed class PersianNumberParserColloquialTests
{
    private readonly PersianNumberParser _parser = new();

    [Theory]
    [InlineData("یه", 1)]
    [InlineData("شیش", 6)]
    [InlineData("هیجده", 18)]
    public void Should_Parse_Spoken_Forms(
        string input,
        decimal expected)
    {
        _parser.TryParse(input, out var result).Should().BeTrue();

        result!.Value.Should().Be(expected);
    }
}
