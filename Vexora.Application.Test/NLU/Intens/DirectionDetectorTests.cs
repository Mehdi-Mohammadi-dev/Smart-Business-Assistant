using FluentAssertions;
using Vexora.Application.NLU.Intent;
using Vexora.Application.NLU.Models.Intents;
using Xunit;

namespace Vexora.Application.Tests.NLU.Intent;

public sealed class DirectionDetectorTests
{
    [Theory]
    [InlineData("علی 500 هزار تومان داد")]
    [InlineData("از علی 3 میلیون گرفتم")]
    [InlineData("علی 500 هزار پرداخت کرد")]
    [InlineData("علی بهم 1 میلیون داد")]
    [InlineData("علی 200 هزار واریز کرد")]
    [InlineData("یک چک 50 میلیونی از حسین گرفتم")]
    [InlineData("علی یه چک داد")]
    [InlineData("از رضا 2 میلیون دریافت کردم")]
    [InlineData("علی 100 هزار تومان کارت به کارت کرد")]
    public void Should_Detect_Received(
        string text)
    {
        DirectionDetector.Detect(text).Should().Be(DirectionHint.Received);
    }

    [Theory]
    [InlineData("به علی 500 هزار دادم")]
    [InlineData("برای علی 3 میلیون واریز کردم")]
    [InlineData("علی 500 هزار گرفت")]
    [InlineData("علی 1 میلیون دریافت کرد")]
    [InlineData("به رضا چک دادم")]
    [InlineData("برای علی یک چک صادر کردم")]
    [InlineData("به علی 2 میلیون پرداخت کردم")]
    [InlineData("برای علی 100 هزار تومان کارت به کارت کردم")]
    public void Should_Detect_Given(
        string text)
    {
        DirectionDetector.Detect(text).Should().Be(DirectionHint.Given);
    }

    [Theory]
    [InlineData("ثبت پرداخت 500 هزار از علی")]
    [InlineData("سلام")]
    [InlineData("علی 500 هزار داد و 200 هزار گرفت")]
    public void Should_Return_Unspecified_When_Not_Clear(
        string text)
    {
        DirectionDetector.Detect(text).Should().Be(DirectionHint.Unspecified);
    }
}
