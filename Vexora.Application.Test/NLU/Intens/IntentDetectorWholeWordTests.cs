using FluentAssertions;
using Vexora.Application.NLU.Intent;
using Vexora.Application.NLU.Models.Intents;
using Xunit;

namespace Vexora.Application.Tests.NLU.Intent;

public sealed class IntentDetectorWholeWordTests
{
    private readonly IntentDetector _detector = new();

    [Theory]
    [InlineData("علی به دادگاه رفت")]
    [InlineData("علی چکش خرید")]
    [InlineData("علی گرفتار شد")]
    [InlineData("پرداختی ها را ببین")]
    [InlineData("سلام حالت چطوره")]
    public void Should_Not_Match_Keywords_Inside_Other_Words(
        string text)
    {
        _detector.Detect(text).Intent.Should().Be(IntentType.Unknown);
    }

    [Theory]
    [InlineData("چک سی میلیون تومانی از احمد رضایی دریافت کردم")]
    [InlineData("علی یه چک 30 میلیونی داد")]
    [InlineData("چک گرفتم از علی")]
    [InlineData("از رضا یه چکی گرفتم")]
    [InlineData("علی با چک 5 میلیون پرداخت کرد")]
    [InlineData("چک 10 میلیونی علی رو واریز کرد")]
    public void Should_Prefer_Check_When_The_Word_Check_Exists(
        string text)
    {
        var result = _detector.Detect(text);

        result.Intent.Should().Be(IntentType.RegisterCheck);
        result.Confidence.Should().BeGreaterThan(0.6);
    }

    [Theory]
    [InlineData("علی 500 هزار تومان پرداخت کرد")]
    [InlineData("از علی 3 میلیون گرفتم")]
    [InlineData("علی 200 هزار واریز کرد")]
    [InlineData("علی بهم 1 میلیون داد")]
    [InlineData("علی 100 هزار تومان کارت به کارت کرد")]
    public void Should_Detect_Payment_Without_The_Word_Check(
        string text)
    {
        _detector.Detect(text).Intent.Should().Be(IntentType.RegisterPayment);
    }
}
