using System.Globalization;
using Vexora.Application.NLU.Extractions.Dates;
using Xunit;

namespace Vexora.Application.Test.NLU.Extractions.Dates;

public sealed class PersianDateExtractorTests
{
    private readonly PersianDateExtractor _extractor =
        new(new FixedTimeProvider(new DateTimeOffset(2026, 10, 24, 9, 0, 0, TimeSpan.Zero)));

    [Fact]
    public void Extract_WhenPersianMonthWithoutYear_ShouldReturnDate()
    {
        var result = _extractor.Extract(
            "برای 15 مهر بهم داد");

        Assert.NotNull(result);

        var calendar = new PersianCalendar();

        Assert.Equal(
            7,
            calendar.GetMonth(result.Value));

        Assert.Equal(
            15,
            calendar.GetDayOfMonth(result.Value));
    }

    [Fact]
    public void Extract_WhenPersianMonthWithYear_ShouldReturnDate()
    {
        var result = _extractor.Extract(
            "برای 15 مهر 1405 بهم داد");

        Assert.NotNull(result);

        var calendar = new PersianCalendar();

        Assert.Equal(
            1405,
            calendar.GetYear(result.Value));

        Assert.Equal(
            7,
            calendar.GetMonth(result.Value));

        Assert.Equal(
            15,
            calendar.GetDayOfMonth(result.Value));
    }

    [Fact]
    public void Extract_WhenFullNumericPersianDate_ShouldReturnDate()
    {
        var result = _extractor.Extract(
            "1405/07/15");

        Assert.NotNull(result);

        var calendar = new PersianCalendar();

        Assert.Equal(
            1405,
            calendar.GetYear(result.Value));

        Assert.Equal(
            7,
            calendar.GetMonth(result.Value));

        Assert.Equal(
            15,
            calendar.GetDayOfMonth(result.Value));
    }

    [Fact]
    public void Extract_WhenShortNumericDate_ShouldReturnDate()
    {
        var result = _extractor.Extract(
            "برای 15/07");

        Assert.NotNull(result);

        var calendar = new PersianCalendar();

        Assert.Equal(
            7,
            calendar.GetMonth(result.Value));

        Assert.Equal(
            15,
            calendar.GetDayOfMonth(result.Value));
    }

    [Fact]
    public void Extract_WhenTomorrow_ShouldReturnTomorrow()
    {
        var result = _extractor.Extract(
            "برای فردا");

        Assert.Equal(
            new DateTime(2026, 10, 25),
            result);
    }

    [Fact]
    public void Extract_WhenNoDateExists_ShouldReturnNull()
    {
        var result = _extractor.Extract(
            "مهدی 20 میلیون بهم داد");

        Assert.Null(result);
    }
}
