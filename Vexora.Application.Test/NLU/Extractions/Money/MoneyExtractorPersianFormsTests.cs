using FluentAssertions;
using Vexora.Application.NLU.Extractions;
using Vexora.Application.NLU.Models.Entities;
using Vexora.Application.NLU.Models.Money;
using Vexora.Application.NLU.Normalizations.Parsing.Money;
using Vexora.Application.NLU.Parsing.Number;
using Xunit;

namespace Vexora.Application.Test.NLU.Extractions.Money;

public sealed class MoneyExtractorPersianFormsTests
{
    private readonly MoneyExtractor _extractor =
        new(new MoneyParser(new PersianNumberParser()));

    [Theory]
    [InlineData("علی 500 هزار داد", 500_000)]
    [InlineData("علی ۵۰۰ هزار داد", 500_000)]
    [InlineData("علی 500 هزار تومان داد", 500_000)]
    [InlineData("علی ده هزار تومان داد", 10_000)]
    [InlineData("علی هزار تومان داد", 1_000)]
    [InlineData("علی 500,000 تومان داد", 500_000)]
    [InlineData("علی ۵۰۰٬۰۰۰ تومان داد", 500_000)]
    [InlineData("علی ۵۰۰،۰۰۰ تومان داد", 500_000)]
    [InlineData("علی 1.500.000 تومان داد", 1_500_000)]
    public void Should_Extract_Thousands_In_Different_Writing_Styles(
        string text,
        decimal expected)
    {
        var money = ExtractSingle(text);

        money.Amount.Should().Be(expected);
        money.Unit.Should().Be(MoneyUnit.Toman);
    }

    [Theory]
    [InlineData("چک 50 میلیونی از حسین", 50_000_000)]
    [InlineData("چک 10 میلیونی علی", 10_000_000)]
    [InlineData("چک علی 5 میلیون", 5_000_000)]
    [InlineData("علی یه چک 30 میلیونی داد", 30_000_000)]
    [InlineData("علی یه میلیون داد", 1_000_000)]
    [InlineData("علی یک میلیارد تومان داد", 1_000_000_000)]
    public void Should_Extract_Million_Without_Currency_Word(
        string text,
        decimal expected)
    {
        ExtractSingle(text).Amount.Should().Be(expected);
    }

    [Theory]
    [InlineData("رضا دو میلیون و پانصد هزار تومان واریز کرد", 2_500_000)]
    [InlineData("علی 2 میلیون و 500 هزار تومان داد", 2_500_000)]
    [InlineData("علی سه میلیون و دویست هزار تومان داد", 3_200_000)]
    [InlineData("علی صد و بیست هزار تومان داد", 120_000)]
    [InlineData("علی دو میلیون و پانصد تومان داد", 2_500_000)]
    [InlineData("علی 2 میلیون 500 هزار داد", 2_500_000)]
    public void Should_Extract_Compound_Amounts(
        string text,
        decimal expected)
    {
        ExtractSingle(text).Amount.Should().Be(expected);
    }

    [Theory]
    [InlineData("علی رضایی یک میلیون و نیم تومان داد", 1_500_000)]
    [InlineData("علی دو و نیم میلیون داد", 2_500_000)]
    [InlineData("علی نیم میلیون داد", 500_000)]
    [InlineData("از محمد 2.5 میلیون گرفتم", 2_500_000)]
    [InlineData("علی 1.5 میلیون تومان داد", 1_500_000)]
    public void Should_Extract_Half_And_Decimal_Amounts(
        string text,
        decimal expected)
    {
        ExtractSingle(text).Amount.Should().Be(expected);
    }

    [Fact]
    public void Should_Keep_Rial_Unit()
    {
        var money = ExtractSingle("علی 500 هزار ریال داد");

        money.Amount.Should().Be(500_000);
        money.Unit.Should().Be(MoneyUnit.Rial);
        money.AmountInToman.Should().Be(50_000);
    }

    [Theory]
    [InlineData("چک علی 7 میلیون سررسید 1405/09/15", 7_000_000)]
    [InlineData("چک شماره 123456 به مبلغ 10 میلیون تومان از علی", 10_000_000)]
    [InlineData("چک 2 میلیون تومانی از رضا با سررسید 15 آذر", 2_000_000)]
    [InlineData("علی 2 میلیون برای 20 دی ماه", 2_000_000)]
    [InlineData("چک 5 میلیون علی 3 ماه دیگه", 5_000_000)]
    [InlineData("علی 3 میلیون داد شماره پیگیری 98765432", 3_000_000)]
    public void Should_Not_Treat_Dates_Check_Numbers_And_Durations_As_Money(
        string text,
        decimal expected)
    {
        var result = _extractor.Extract(text);

        result.Should().ContainSingle();

        ((MoneyAmount)result.Single().Value!).Amount.Should().Be(expected);
    }

    [Theory]
    [InlineData("علی سه ماه دیگه میاد")]
    [InlineData("علی ده روز دیگه میاد")]
    [InlineData("نه ممنون")]
    [InlineData("سیب و سیمان")]
    [InlineData("چک 15 آذر")]
    [InlineData("سررسید 1405/09/15")]
    public void Should_Return_Empty_For_Words_That_Only_Look_Like_Numbers(
        string text)
    {
        _extractor.Extract(text).Should().BeEmpty();
    }

    [Fact]
    public void Should_Return_Every_Amount_When_Text_Has_Two_Amounts()
    {
        var result = _extractor.Extract(
            "علی 2 میلیون تومان داد و 500 هزار تومان بدهکار شد");

        result.Should().HaveCount(2);
    }

    [Fact]
    public void Should_Give_Lower_Confidence_When_Currency_Word_Is_Missing()
    {
        var withCurrency = _extractor.Extract("علی 500 هزار تومان داد").Single();
        var withoutCurrency = _extractor.Extract("علی 500 هزار داد").Single();

        withoutCurrency.Confidence.Should().BeLessThan(withCurrency.Confidence);
    }

    private MoneyAmount ExtractSingle(string text)
    {
        var result = _extractor.Extract(text);

        result.Should().ContainSingle();

        var entity = result.Single();

        entity.Type.Should().Be(EntityType.Money);

        return (MoneyAmount)entity.Value!;
    }
}
