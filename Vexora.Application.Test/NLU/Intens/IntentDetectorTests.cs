using FluentAssertions;
using Vexora.Application.NLU.Models.Intents;
using Voxora.Application.NLU.Intent;
using Xunit;

namespace Voxora.Application.Tests.NLU.Intent;

public sealed class IntentDetectorTests
{
    private readonly IntentDetector _detector = new();

    [Theory]
    [InlineData(
        "علی 150 تومان داد",
        IntentType.RegisterPayment)]

    [InlineData(
        "علی 150 تومن پرداخت کرد",
        IntentType.RegisterPayment)]
    
    [InlineData(
        "به علی 10 میلیون جنس دادم",
        IntentType.RegisterSale)]

    [InlineData(
        "یک چک 50 میلیونی از حسین گرفتم",
        IntentType.RegisterCheck)]

    [InlineData(
        "علی چقدر بدهکاره",
        IntentType.GetCustomerBalance)]

    [InlineData(
        "تراکنش های علی رو نشون بده",
        IntentType.GetCustomerTransactions)]

    [InlineData(
        "فروش امروز چقدر بوده",
        IntentType.GetTodaySales)]

    public void Should_Detect_Intent(
        string text,
        IntentType expected)
    {
        var result = _detector.Detect(text);

        result.Intent.Should().Be(expected);
        result.Confidence.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData("")]
    [InlineData("سلام")]
    [InlineData("هوا امروز خوبه")]
    public void Should_Return_Unknown(
        string text)
    {
        var result = _detector.Detect(text);

        result.Intent.Should().Be(IntentType.Unknown);
        result.Confidence.Should().Be(0);
    }
}