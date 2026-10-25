using FluentAssertions;
using Vexora.Application.Common;
using Xunit;

namespace Vexora.Application.Test.Ledger;

public sealed class PersianDateFormatterTests
{
    [Theory]
    [InlineData(2026, 10, 24, "1405/08/02")]
    [InlineData(2026, 12, 6, "1405/09/15")]
    [InlineData(2026, 9, 23, "1405/07/01")]
    [InlineData(2026, 10, 22, "1405/07/30")]
    [InlineData(2026, 10, 23, "1405/08/01")]
    [InlineData(2021, 3, 21, "1400/01/01")]
    [InlineData(2020, 3, 20, "1399/01/01")]
    public void ToPersian_KnownDates(int year, int month, int day, string expected)
    {
        PersianDateFormatter.ToPersian(new DateTime(year, month, day))
            .Should().Be(expected);
    }

    [Fact]
    public void ToPersian_LeapYearEsfand_HasThirtyDays()
    {
        PersianDateFormatter.ToPersian(new DateTime(2025, 3, 20)).Should().Be("1403/12/30");
        PersianDateFormatter.ToPersian(new DateTime(2025, 3, 21)).Should().Be("1404/01/01");
    }

    [Fact]
    public void ToPersian_CommonYearEsfand_HasTwentyNineDays()
    {
        PersianDateFormatter.ToPersian(new DateTime(2024, 3, 19)).Should().Be("1402/12/29");
        PersianDateFormatter.ToPersian(new DateTime(2024, 3, 20)).Should().Be("1403/01/01");
        PersianDateFormatter.ToPersian(new DateTime(2026, 3, 20)).Should().Be("1404/12/29");
        PersianDateFormatter.ToPersian(new DateTime(2026, 3, 21)).Should().Be("1405/01/01");
    }

    [Fact]
    public void ToPersian_IgnoresTimeOfDay()
    {
        PersianDateFormatter.ToPersian(new DateTime(2026, 10, 24, 23, 59, 59))
            .Should().Be("1405/08/02");
    }

    [Fact]
    public void StartOfMonth_ReturnsFirstDayOfPersianMonth()
    {
        PersianDateFormatter.StartOfMonth(new DateTime(2026, 10, 24))
            .Should().Be(new DateTime(2026, 10, 23));

        PersianDateFormatter.StartOfMonth(new DateTime(2026, 10, 23))
            .Should().Be(new DateTime(2026, 10, 23));

        PersianDateFormatter.StartOfMonth(new DateTime(2026, 10, 22))
            .Should().Be(new DateTime(2026, 9, 23));
    }

    [Theory]
    [InlineData(1399, true)]
    [InlineData(1403, true)]
    [InlineData(1408, true)]
    [InlineData(1400, false)]
    [InlineData(1402, false)]
    [InlineData(1404, false)]
    [InlineData(1405, false)]
    public void IsLeapYear_MatchesOfficialCalendar(int year, bool expected)
    {
        PersianDateFormatter.IsLeapYear(year).Should().Be(expected);
    }
}
