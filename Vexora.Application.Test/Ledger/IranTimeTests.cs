using FluentAssertions;
using Vexora.Application.Common;
using Xunit;

namespace Vexora.Application.Test.Ledger;

public sealed class IranTimeTests
{
    [Fact]
    public void Today_BeforeIranMidnight_IsSameDay()
    {
        var clock = new LedgerFixedClock(new DateTimeOffset(2026, 10, 24, 20, 29, 59, TimeSpan.Zero));

        IranTime.Today(clock).Should().Be(new DateTime(2026, 10, 24));
    }

    [Fact]
    public void Today_AtIranMidnight_IsNextDay()
    {
        var clock = new LedgerFixedClock(new DateTimeOffset(2026, 10, 24, 20, 30, 0, TimeSpan.Zero));

        IranTime.Today(clock).Should().Be(new DateTime(2026, 10, 25));
    }

    [Fact]
    public void StartOfDayUtc_IsThreeAndHalfHoursBeforeUtcMidnight()
    {
        IranTime.StartOfDayUtc(new DateTime(2026, 10, 24))
            .Should().Be(new DateTimeOffset(2026, 10, 23, 20, 30, 0, TimeSpan.Zero));
    }

    [Fact]
    public void ToIranDate_ConvertsFromUtcInstant()
    {
        IranTime.ToIranDate(new DateTimeOffset(2026, 10, 24, 21, 0, 0, TimeSpan.Zero))
            .Should().Be(new DateTime(2026, 10, 25));
    }
}
