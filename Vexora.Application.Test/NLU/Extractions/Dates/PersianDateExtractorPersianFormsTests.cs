using System.Globalization;
using FluentAssertions;
using Vexora.Application.NLU.Extractions.Dates;
using Xunit;

namespace Vexora.Application.Test.NLU.Extractions.Dates;

public sealed class PersianDateExtractorPersianFormsTests
{
    private static readonly PersianCalendar Calendar = new();

    private static readonly DateTimeOffset NowUtc =
        new(2026, 10, 24, 9, 0, 0, TimeSpan.Zero);

    private readonly PersianDateExtractor _extractor =
        new(new FixedTimeProvider(NowUtc));

    private static DateTime P(int year, int month, int day)
        => Calendar.ToDateTime(year, month, day, 0, 0, 0, 0);

    [Theory]
    [InlineData("برای فردا", 1405, 8, 3)]
    [InlineData("پس فردا", 1405, 8, 4)]
    [InlineData("پس‌فردا", 1405, 8, 4)]
    [InlineData("پسفردا", 1405, 8, 4)]
    public void Should_Extract_Tomorrow_And_The_Day_After(
        string text, int year, int month, int day)
    {
        _extractor.Extract(text).Should().Be(P(year, month, day));
    }

    [Theory]
    [InlineData("سه ماه دیگه", 1405, 11, 2)]
    [InlineData("3 ماه دیگه", 1405, 11, 2)]
    [InlineData("۳ ماه بعد", 1405, 11, 2)]
    [InlineData("ماه بعد", 1405, 9, 2)]
    [InlineData("ماه آینده", 1405, 9, 2)]
    [InlineData("دو ماه بعد", 1405, 10, 2)]
    [InlineData("یه سال دیگه", 1406, 8, 2)]
    [InlineData("هفته دیگه", 1405, 8, 9)]
    [InlineData("یک هفته دیگه", 1405, 8, 9)]
    [InlineData("یه هفته بعد", 1405, 8, 9)]
    [InlineData("دو هفته دیگه", 1405, 8, 16)]
    [InlineData("ده روز دیگه", 1405, 8, 12)]
    [InlineData("15 روز دیگه", 1405, 8, 17)]
    [InlineData("بیست و پنج روز دیگه", 1405, 8, 27)]
    [InlineData("45 روز دیگه", 1405, 9, 17)]
    public void Should_Extract_Relative_Durations(
        string text, int year, int month, int day)
    {
        _extractor.Extract(text).Should().Be(P(year, month, day));
    }

    [Theory]
    [InlineData("پانزدهم آذر", 1405, 9, 15)]
    [InlineData("بیستم دی ماه", 1405, 10, 20)]
    [InlineData("سی ام آذر", 1405, 9, 30)]
    [InlineData("بیست و پنجم آبان", 1405, 8, 25)]
    [InlineData("اول اسفند", 1405, 12, 1)]
    [InlineData("اول ماه آذر", 1405, 9, 1)]
    [InlineData("15ام آذر", 1405, 9, 15)]
    [InlineData("15 آذر ماه", 1405, 9, 15)]
    [InlineData("20 دی ماه", 1405, 10, 20)]
    [InlineData("۲۰ دی", 1405, 10, 20)]
    public void Should_Extract_Day_And_Month_In_Words_And_Digits(
        string text, int year, int month, int day)
    {
        _extractor.Extract(text).Should().Be(P(year, month, day));
    }

    [Theory]
    [InlineData("15 مهر", 1405, 7, 15)]
    [InlineData("31 شهریور", 1405, 6, 31)]
    [InlineData("اول فروردین", 1406, 1, 1)]
    [InlineData("15 تیر", 1406, 4, 15)]
    [InlineData("15 مهر 1404", 1404, 7, 15)]
    [InlineData("15 آذر 1400 تومان", 1405, 9, 15)]
    public void Should_Move_A_Date_Without_Year_To_Next_Year_Only_When_It_Is_Far_In_The_Past(
        string text, int year, int month, int day)
    {
        _extractor.Extract(text).Should().Be(P(year, month, day));
    }

    [Theory]
    [InlineData("1405/09/15", 1405, 9, 15)]
    [InlineData("1405-09-15", 1405, 9, 15)]
    [InlineData("۱۴۰۵/۰۹/۱۵", 1405, 9, 15)]
    [InlineData("سررسید 1405/12/10", 1405, 12, 10)]
    [InlineData("15/09", 1405, 9, 15)]
    public void Should_Extract_Numeric_Dates(
        string text, int year, int month, int day)
    {
        _extractor.Extract(text).Should().Be(P(year, month, day));
    }

    [Fact]
    public void Should_Accept_A_Gregorian_Date()
    {
        _extractor.Extract("2026/12/06").Should().Be(new DateTime(2026, 12, 6));
    }

    [Theory]
    [InlineData("1405/13/01")]
    [InlineData("1405/09/31")]
    [InlineData("1405/12/30")]
    [InlineData("31 آذر")]
    [InlineData("31 مهر")]
    [InlineData("30 اسفند 1405")]
    [InlineData("شماره 12-34567")]
    public void Should_Return_Null_For_Dates_That_Do_Not_Exist(
        string text)
    {
        _extractor.Extract(text).Should().BeNull();
    }

    [Theory]
    [InlineData("چهارشنبه", 1405, 8, 6)]
    [InlineData("جمعه", 1405, 8, 8)]
    [InlineData("شنبه", 1405, 8, 9)]
    [InlineData("یکشنبه", 1405, 8, 3)]
    [InlineData("یک شنبه", 1405, 8, 3)]
    [InlineData("دوشنبه", 1405, 8, 4)]
    [InlineData("سه شنبه", 1405, 8, 5)]
    [InlineData("سه‌شنبه", 1405, 8, 5)]
    [InlineData("پنجشنبه", 1405, 8, 7)]
    [InlineData("پنج شنبه", 1405, 8, 7)]
    [InlineData("چهارشنبه این هفته", 1405, 8, 6)]
    [InlineData("چهارشنبه هفته بعد", 1405, 8, 13)]
    [InlineData("جمعه هفته دیگه", 1405, 8, 15)]
    public void Should_Extract_Weekdays_As_The_Next_Occurrence(
        string text, int year, int month, int day)
    {
        _extractor.Extract(text).Should().Be(P(year, month, day));
    }

    [Theory]
    [InlineData("آخر ماه", 1405, 8, 30)]
    [InlineData("آخر این ماه", 1405, 8, 30)]
    [InlineData("آخر ماه بعد", 1405, 9, 30)]
    [InlineData("اول ماه بعد", 1405, 9, 1)]
    [InlineData("اول ماه آینده", 1405, 9, 1)]
    public void Should_Extract_Month_Boundaries(
        string text, int year, int month, int day)
    {
        _extractor.Extract(text).Should().Be(P(year, month, day));
    }

    [Theory]
    [InlineData("چک علی 5 میلیون برای پانزدهم آذر", 1405, 9, 15)]
    [InlineData("چک علی 5 میلیون هفته دیگه", 1405, 8, 9)]
    [InlineData("چک علی 5 میلیون ماه بعد", 1405, 9, 2)]
    [InlineData("چک 500 هزار تومانی از علی هفته بعد", 1405, 8, 9)]
    [InlineData("علی 2 میلیون و 500 هزار تومان داد ده روز دیگه", 1405, 8, 12)]
    [InlineData("چک 500 هزار تومانی از علی، سه ماه دیگه", 1405, 11, 2)]
    [InlineData("چک 2 میلیون تومانی از رضا با سررسید 15 آذر", 1405, 9, 15)]
    [InlineData("از رضا یک چک 20 میلیون تومانی گرفتم سررسید 1405/09/15", 1405, 9, 15)]
    public void Should_Find_The_Date_Inside_A_Full_Sentence(
        string text, int year, int month, int day)
    {
        _extractor.Extract(text).Should().Be(P(year, month, day));
    }

    [Theory]
    [InlineData("مهدی 20 میلیون بهم داد")]
    [InlineData("علی امروز 500 هزار داد")]
    [InlineData("علی 30 روز مهلت داد")]
    [InlineData("علی دیروز اومد")]
    [InlineData("هفته")]
    [InlineData("ماه")]
    [InlineData("چک 10 میلیونی")]
    [InlineData("مهر")]
    [InlineData("دی")]
    [InlineData("ده")]
    public void Should_Return_Null_When_There_Is_No_Date(
        string text)
    {
        _extractor.Extract(text).Should().BeNull();
    }

    [Fact]
    public void Should_Use_Iran_Time_To_Decide_What_Today_Is()
    {
        var lateEvening = new PersianDateExtractor(
            new FixedTimeProvider(new DateTimeOffset(2026, 10, 24, 21, 0, 0, TimeSpan.Zero)));

        lateEvening.Extract("فردا").Should().Be(new DateTime(2026, 10, 26));
    }

    [Fact]
    public void Should_Return_A_Date_Without_Time_Zone()
    {
        _extractor.Extract("فردا")!.Value.Kind.Should().Be(DateTimeKind.Unspecified);
    }
}
