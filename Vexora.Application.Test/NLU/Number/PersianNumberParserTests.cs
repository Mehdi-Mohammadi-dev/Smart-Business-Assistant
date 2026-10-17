using FluentAssertions;
using Vexora.Application.NLU.Parsing.Number;
using Xunit;

namespace Vexora.Application.Tests.NLU.Number;

public sealed class PersianNumberParserTests
{
    private readonly PersianNumberParser _parser = new();

    [Theory]
    [InlineData("یک", 1)]
    [InlineData("دو", 2)]
    [InlineData("ده", 10)]
    [InlineData("بیست", 20)]
    [InlineData("صد", 100)]
    [InlineData("پانصد", 500)]
    [InlineData("هزار", 1000)]
    [InlineData("یک هزار", 1000)]
    [InlineData("پانصد هزار", 500000)]
    [InlineData("یک میلیون", 1000000)]
    [InlineData("دو میلیون", 2000000)]
    [InlineData("دو میلیون و پانصد هزار", 2500000)]
    [InlineData("یک میلیارد", 1000000000)]
    public void Should_Parse_Persian_Numbers(
        string input,
        decimal expected)
    {
        var success = _parser.TryParse(
            input,
            out var result);

        success.Should().BeTrue();
        result.Should().NotBeNull();
        result!.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData("150", 150)]
    [InlineData("1.5", 1.5)]
    [InlineData("1500", 1500)]
    [InlineData("1,500", 1500)]
    public void Should_Parse_Numeric_Values(
        string input,
        decimal expected)
    {
        var success = _parser.TryParse(
            input,
            out var result);

        success.Should().BeTrue();
        result!.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData("دو و نیم", 2.5)]
    [InlineData("یک و نیم", 1.5)]
    [InlineData("سه و نیم", 3.5)]
    public void Should_Parse_Half_Values(
        string input,
        decimal expected)
    {
        var success = _parser.TryParse(
            input,
            out var result);

        success.Should().BeTrue();
        result!.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("سلام")]
    [InlineData("علی")]
    [InlineData("امروز")]
    public void Should_Return_False_For_Invalid_Input(
        string input)
    {
        var success = _parser.TryParse(
            input,
            out var result);

        success.Should().BeFalse();
        result.Should().BeNull();
    }
}
