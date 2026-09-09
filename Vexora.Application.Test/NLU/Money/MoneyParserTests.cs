using FluentAssertions;
using Vexora.Application.NLU.Models.Money;
using Vexora.Application.NLU.Normalizations.Parsing.Money;
using Voxora.Application.NLU.Parsing.Number;
using Xunit;

namespace Voxora.Application.Tests.NLU.Entities.Money;

public sealed class MoneyParserTests
{
    private readonly MoneyParser _parser;

    public MoneyParserTests()
    {
        _parser = new MoneyParser(
            new PersianNumberParser());
    }

    [Theory]
    [InlineData("150", 150, MoneyUnit.Toman)]
    [InlineData("150 تومان", 150, MoneyUnit.Toman)]
    [InlineData("150 تومن", 150, MoneyUnit.Toman)]
    [InlineData("یک میلیون", 1000000, MoneyUnit.Toman)]
    [InlineData("یک میلیون تومان", 1000000, MoneyUnit.Toman)]
    [InlineData("دو میلیون و پانصد هزار تومان", 2500000, MoneyUnit.Toman)]
    [InlineData("دو میلیون ریال", 2000000, MoneyUnit.Rial)]
    public void Should_Parse_Money(
        string input,
        decimal expectedAmount,
        MoneyUnit expectedUnit)
    {
        var success = _parser.TryParse(
            input,
            out var result);

        success.Should().BeTrue();
        result.Should().NotBeNull();

        result!.Amount.Should().Be(expectedAmount);
        result.Unit.Should().Be(expectedUnit);
    }

    [Theory]
    [InlineData("سلام")]
    [InlineData("علی")]
    [InlineData("")]
    public void Should_Return_False_For_Invalid_Money(
        string input)
    {
        var success = _parser.TryParse(
            input,
            out var result);

        success.Should().BeFalse();
        result.Should().BeNull();
    }
}