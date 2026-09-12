using FluentAssertions;
using Vexora.Application.NLU.Extractions;
using Vexora.Application.NLU.Models.Entities;
using Vexora.Application.NLU.Models.Money;
using Vexora.Application.NLU.Normalizations.Parsing.Money;
using Voxora.Application.NLU.Parsing.Number;
using Xunit;

namespace Vexora.Application.Test.NLU.Extractions.Money;

public sealed class MoneyExtractorTests
{
    private readonly MoneyExtractor _extractor;

    public MoneyExtractorTests()
    {
        var numberParser = new PersianNumberParser();
        var moneyParser = new MoneyParser(numberParser);

        _extractor = new MoneyExtractor(moneyParser);
    }

    [Fact]
    public void Should_Extract_Money_With_Toman()
    {
        var result = _extractor.Extract(
            "علی 150 تومان داد");

        result.Should().ContainSingle();

        var entity = result.Single();

        entity.Type.Should().Be(EntityType.Money);
        entity.RawValue.Should().Be("150 تومان");
        entity.Value.Should().BeOfType<MoneyAmount>();

        var money = (MoneyAmount)entity.Value!;

        money.Amount.Should().Be(150);
        money.Unit.Should().Be(MoneyUnit.Toman);
    }

    [Fact]
    public void Should_Extract_Money_With_Persian_Digits()
    {
        var result = _extractor.Extract(
            "علی ۱۵۰ تومان داد");

        result.Should().ContainSingle();

        var money = (MoneyAmount)result.Single().Value!;

        money.Amount.Should().Be(150);
    }

    [Fact]
    public void Should_Extract_Million()
    {
        var result = _extractor.Extract(
            "به علی 10 میلیون تومان جنس دادم");

        result.Should().ContainSingle();

        var money = (MoneyAmount)result.Single().Value!;

        money.Amount.Should().Be(10_000_000);
    }

    [Fact]
    public void Should_Extract_Rial()
    {
        var result = _extractor.Extract(
            "علی 500000 ریال داد");

        result.Should().ContainSingle();

        var money = (MoneyAmount)result.Single().Value!;

        money.Amount.Should().Be(500_000);
        money.Unit.Should().Be(MoneyUnit.Rial);
    }

    [Fact]
    public void Should_Extract_Bare_Number_As_Low_Confidence_Money()
    {
        var result = _extractor.Extract(
            "علی 150 داد");

        result.Should().ContainSingle();

        var entity = result.Single();

        entity.Type.Should().Be(EntityType.Money);
        entity.Confidence.Should().BeLessThan(1);
    }

    [Fact]
    public void Should_Return_Empty_When_No_Money_Exists()
    {
        var result = _extractor.Extract(
            "علی امروز آمد");

        result.Should().BeEmpty();
    }
}
