using FluentAssertions;
using Moq;
using Vexora.Application.Ledger.Models;
using Vexora.Application.Ledger.Repositories;
using Vexora.Application.Ledger.Summary;
using Vexora.Application.Services.Interfaces.CurrentUser;
using Xunit;

using Vexora.Application.Test.Ledger.LedgerSummaryServiceTestsHelpers;

namespace Vexora.Application.Test.Ledger;

public sealed class LedgerSummaryServiceTests
{



    private static LedgerSummaryService Create(FakeSummaryReader summaryReader, int customers = 18)
    {
        var currentUser = new Mock<ICurrentUser>();
        currentUser.Setup(x => x.UserId).Returns(1);

        return new LedgerSummaryService(
            summaryReader,
            new FakeCustomerReader(customers),
            currentUser.Object,
            new LedgerFixedClock(new DateTimeOffset(2026, 10, 24, 9, 0, 0, TimeSpan.Zero)));
    }

    [Fact]
    public async Task Get_UsesIranTodayAndPersianMonthBounds()
    {
        var reader = new FakeSummaryReader(null, null);

        var summary = await Create(reader).GetAsync();

        summary.Today.Should().Be(new DateOnly(2026, 10, 24));
        summary.TodayPersian.Should().Be("1405/08/02");
        summary.CustomersCount.Should().Be(18);

        reader.SeenToday.Should().Be(new DateTime(2026, 10, 24));
        reader.SeenDueSoonDays.Should().Be(7);
        reader.SeenBounds.Month.Should().Be(new DateTimeOffset(2026, 10, 22, 20, 30, 0, TimeSpan.Zero));
        reader.SeenBounds.Today.Should().Be(new DateTimeOffset(2026, 10, 23, 20, 30, 0, TimeSpan.Zero));
        reader.SeenBounds.Tomorrow.Should().Be(new DateTimeOffset(2026, 10, 24, 20, 30, 0, TimeSpan.Zero));
    }

    [Fact]
    public async Task Get_NoData_ReturnsZeros()
    {
        var summary = await Create(new FakeSummaryReader(null, null)).GetAsync();

        summary.Checks.PendingCount.Should().Be(0);
        summary.Checks.PendingTotal.Should().Be(0);
        summary.Payments.MonthTotal.Should().Be(0);
        summary.Payments.TodayTotal.Should().Be(0);
    }

    [Fact]
    public async Task Get_MapsAggregates()
    {
        var checks = new CheckPendingAggregate
        {
            PendingCount = 7,
            PendingTotal = 85_000_000m,
            OverdueCount = 1,
            OverdueTotal = 5_000_000m,
            DueSoonCount = 2,
            DueSoonTotal = 20_000_000m,
            NotRegisteredCount = 4
        };

        var payments = new PaymentPeriodAggregate { MonthCount = 9, MonthTotal = 12_000_000m, TodayTotal = 1_500_000m };

        var summary = await Create(new FakeSummaryReader(checks, payments)).GetAsync();

        summary.Checks.DueWithin7Count.Should().Be(2);
        summary.Checks.DueWithin7Total.Should().Be(20_000_000m);
        summary.Checks.NotRegisteredPendingCount.Should().Be(4);
        summary.Payments.MonthCount.Should().Be(9);
        summary.Payments.TodayTotal.Should().Be(1_500_000m);
    }
}
