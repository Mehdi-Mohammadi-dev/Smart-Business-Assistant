using FluentAssertions;
using Vexora.Application.Common;
using Vexora.Application.Test.NLU.Extractions.Dates;
using Xunit;

namespace Vexora.Application.Test.Zero;

public sealed class IranTimeAndPersianDateTests
{
    [Fact]
    public void Today_Should_Follow_Iran_Time()
    {
        var evening = new FixedTimeProvider(new DateTimeOffset(2026, 10, 24, 21, 0, 0, TimeSpan.Zero));
        var noon = new FixedTimeProvider(new DateTimeOffset(2026, 10, 24, 9, 0, 0, TimeSpan.Zero));

        IranTime.Today(evening).Should().Be(new DateTime(2026, 10, 25));
        IranTime.Today(noon).Should().Be(new DateTime(2026, 10, 24));
    }

    [Fact]
    public void Now_Should_Have_No_Time_Zone()
    {
        var provider = new FixedTimeProvider(new DateTimeOffset(2026, 10, 24, 9, 0, 0, TimeSpan.Zero));

        IranTime.Now(provider).Kind.Should().Be(DateTimeKind.Unspecified);
        IranTime.Now(provider).Hour.Should().Be(12);
        IranTime.Now(provider).Minute.Should().Be(30);
    }

    [Theory]
    [InlineData(2026, 12, 6, "1405/09/15")]
    [InlineData(2026, 10, 24, "1405/08/02")]
    [InlineData(2025, 3, 20, "1403/12/30")]
    [InlineData(2025, 3, 21, "1404/01/01")]
    public void Should_Format_A_Date_In_Persian_Calendar(
        int year, int month, int day, string expected)
    {
        PersianDateFormatter.ToPersian(new DateTime(year, month, day)).Should().Be(expected);
    }

    [Fact]
    public void An_Instant_Should_Be_Converted_To_Iran_Day()
    {
        var lateUtcEvening = new DateTimeOffset(2026, 10, 24, 21, 0, 0, TimeSpan.Zero);

        PersianDateFormatter.ToPersian(lateUtcEvening).Should().Be("1405/08/03");
    }
}
