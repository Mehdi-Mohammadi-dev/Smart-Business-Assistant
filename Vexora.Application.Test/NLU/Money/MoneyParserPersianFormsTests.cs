using FluentAssertions;
using Vexora.Application.NLU.Models.Money;
using Vexora.Application.NLU.Normalizations.Parsing.Money;
using Vexora.Application.NLU.Parsing.Number;
using Xunit;

namespace Vexora.Application.Tests.NLU.Entities.Money;

public sealed class MoneyParserPersianFormsTests
{
    private readonly MoneyParser _parser = new(new PersianNumberParser());

    [Theory]
    [InlineData("500 هزار", 500_000)]
    [InlineData("هزار تومان", 1_000)]
    [InlineData("50 میلیونی", 50_000_000)]
    [InlineData("نیم میلیون", 500_000)]
    [InlineData("یک میلیون و نیم", 1_500_000)]
    [InlineData("دو و نیم میلیون", 2_500_000)]
    [InlineData("2.5 میلیون", 2_500_000)]
    [InlineData("2 میلیون و 500 هزار تومان", 2_500_000)]
    [InlineData("دو میلیون و پانصد", 2_500_000)]
    [InlineData("یک میلیارد و پانصد میلیون", 1_500_000_000)]
    [InlineData("پنج هزار و 500", 5_500)]
    [InlineData("500هزار", 500_000)]
    [InlineData("500هزارتومان", 500_000)]
    [InlineData("یه میلیون", 1_000_000)]
    [InlineData("۵۰۰٬۰۰۰ تومان", 500_000)]
    [InlineData("1,500,000", 1_500_000)]
    public void Should_Parse_Persian_Money_Forms(
        string input,
        decimal expected)
    {
        var success = _parser.TryParse(input, out var result);

        success.Should().BeTrue();
        result!.Amount.Should().Be(expected);
        result.Unit.Should().Be(MoneyUnit.Toman);
    }

    [Theory]
    [InlineData("500 هزار ریال", 500_000)]
    [InlineData("دو میلیون ریالی", 2_000_000)]
    public void Should_Detect_Rial_And_Convert_To_Toman(
        string input,
        decimal expectedAmount)
    {
        _parser.TryParse(input, out var result).Should().BeTrue();

        result!.Unit.Should().Be(MoneyUnit.Rial);
        result.Amount.Should().Be(expectedAmount);
        result.AmountInToman.Should().Be(expectedAmount / 10);
    }

    [Fact]
    public void Toman_Amount_Should_Not_Change_When_Converted()
    {
        var money = new MoneyAmount(150_000, MoneyUnit.Toman);

        money.AmountInToman.Should().Be(150_000);
    }

    [Theory]
    [InlineData("نیم")]
    [InlineData("و")]
    [InlineData("500 علی")]
    [InlineData("تومان")]
    public void Should_Return_False_For_Incomplete_Or_Mixed_Text(
        string input)
    {
        _parser.TryParse(input, out var result).Should().BeFalse();
        result.Should().BeNull();
    }
}
