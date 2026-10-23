using FluentAssertions;
using Vexora.Application.NLU.Extractions.Customers;
using Xunit;

namespace Vexora.Application.Test.NLU.Extractions.Customers;

public sealed class CustomerExtractorPersianFormsTests
{
    private readonly CustomerExtractor _extractor = new();

    [Theory]
    [InlineData("علی 100 هزار تومان کارت به کارت کرد", "علی")]
    [InlineData("علی 500 هزار تومان نقد داد", "علی")]
    [InlineData("علی 500 هزار داد", "علی")]
    [InlineData("علی 150 تومان داد", "علی")]
    [InlineData("پرداخت 500 هزار تومان از علی", "علی")]
    [InlineData("از علی رضایی 3 میلیون تومان گرفتم", "علی رضایی")]
    [InlineData("علی رضایی بنا 1 میلیون داد", "علی رضایی بنا")]
    [InlineData("حاج احمد رضایی 2 میلیون تومان داد", "حاج احمد رضایی")]
    [InlineData("آقای احمدی 200 هزار تومان پرداخت کرد", "احمدی")]
    [InlineData("خانم کریمی 1 میلیون داد", "کریمی")]
    public void Should_Extract_The_Payer_Name_From_Payment_Sentences(
        string text,
        string expected)
    {
        ExtractName(text).Should().Be(expected);
    }

    [Theory]
    [InlineData("چک 2 میلیون تومانی از رضا با سررسید 15 آذر", "رضا")]
    [InlineData("چک شماره 123456 به مبلغ 10 میلیون تومان از علی گرفتم", "علی")]
    [InlineData("چک علی 5 میلیون برای پانزدهم آذر", "علی")]
    [InlineData("چک 10 میلیونی علی برای فردا", "علی")]
    [InlineData("علی یه چک 30 میلیونی داد", "علی")]
    [InlineData("چک 50 میلیون از علی پس فردا", "علی")]
    [InlineData("چک 5 میلیون علی 15 آبان", "علی")]
    [InlineData("چک علی 5 میلیون هفته دیگه", "علی")]
    [InlineData("چک 500 هزار تومانی از علی، سه ماه دیگه", "علی")]
    [InlineData("از رضا یک چک 20 میلیون تومانی گرفتم سررسید 1405/09/15", "رضا")]
    [InlineData("چک سی میلیون تومانی از احمد رضایی دریافت کردم تاریخ 1405/12/10", "احمد رضایی")]
    public void Should_Extract_The_Name_From_Check_Sentences_Without_Dates_Or_Words(
        string text,
        string expected)
    {
        ExtractName(text).Should().Be(expected);
    }

    [Theory]
    [InlineData("امروز 500 هزار تومان فروختم")]
    [InlineData("فردا 500 هزار تومان فروختم")]
    [InlineData("500 هزار تومان کارت به کارت شد")]
    [InlineData("چک 5 میلیون تومانی با سررسید 15 آذر")]
    public void Should_Not_Return_A_Name_When_There_Is_No_Customer(
        string text)
    {
        _extractor.Extract(text).Should().BeEmpty();
    }

    [Fact]
    public void Should_Return_Position_Of_The_Name()
    {
        const string text = "چک 5 میلیون از علی رضایی گرفتم";

        var entity = _extractor.Extract(text).Single();

        text.Substring(entity.Start, entity.Length).Should().Be("علی رضایی");
    }

    private string ExtractName(string text)
    {
        var result = _extractor.Extract(text);

        result.Should().ContainSingle();

        return result.Single().RawValue;
    }
}
