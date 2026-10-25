using FluentAssertions;
using Vexora.Application.Ledger.Queries;
using Vexora.Domain.Entities.Payments;
using Vexora.Domain.Enums.TransactionDirections;
using Xunit;

namespace Vexora.Application.Test.Ledger;

public sealed class PaymentQueriesTests
{
    private static readonly TimeSpan Iran = new(3, 30, 0);

    private static Payment At(long userId, DateTimeOffset date, decimal amount = 100_000m, long customerId = 10,
        TransactionDirection direction = TransactionDirection.Received)
        => Payment.Create(userId, customerId, amount, date, direction);

    private static Vexora.Application.Ledger.Payments.PaymentListQuery Query(DateTime? from = null, DateTime? to = null, long? customerId = null)
        => new(customerId, from, to, 1, 20);

    [Fact]
    public void Filter_NeverReturnsOtherUsersPayments()
    {
        var payments = new List<Payment>
        {
            At(1, DateTimeOffset.UtcNow),
            At(2, DateTimeOffset.UtcNow),
            At(2, DateTimeOffset.UtcNow)
        };

        PaymentQueries.Filter(payments.AsQueryable(), 1, Query()).ToList()
            .Should().OnlyContain(x => x.UserId == 1).And.HaveCount(1);
    }

    [Fact]
    public void Filter_ByCustomer()
    {
        var payments = new List<Payment>
        {
            At(1, DateTimeOffset.UtcNow, customerId: 10),
            At(1, DateTimeOffset.UtcNow, customerId: 11)
        };

        PaymentQueries.Filter(payments.AsQueryable(), 1, Query(customerId: 11)).ToList()
            .Should().ContainSingle().Which.CustomerId.Should().Be(11);
    }

    [Fact]
    public void Filter_DayRange_UsesIranDayBoundaries()
    {
        var startOfDay = new DateTimeOffset(2026, 10, 24, 0, 0, 0, Iran);
        var beforeStart = startOfDay.AddSeconds(-1);
        var lastSecond = new DateTimeOffset(2026, 10, 24, 23, 59, 59, Iran);
        var nextDay = new DateTimeOffset(2026, 10, 25, 0, 0, 0, Iran);

        var payments = new List<Payment>
        {
            At(1, beforeStart, 1m),
            At(1, startOfDay, 2m),
            At(1, lastSecond, 3m),
            At(1, nextDay, 4m)
        };

        var day = new DateTime(2026, 10, 24);

        PaymentQueries.Filter(payments.AsQueryable(), 1, Query(day, day)).ToList()
            .Select(x => x.Amount).Should().BeEquivalentTo(new[] { 2m, 3m });
    }

    [Fact]
    public void ByCustomer_SumsOnlyReceived()
    {
        var payments = new List<Payment>
        {
            At(1, DateTimeOffset.UtcNow, 100m),
            At(1, DateTimeOffset.UtcNow, 200m),
            At(1, DateTimeOffset.UtcNow, 900m, direction: TransactionDirection.Paid),
            At(2, DateTimeOffset.UtcNow, 500m)
        };

        var aggregate = PaymentQueries.ByCustomer(payments.AsQueryable(), 1, null).Single();

        aggregate.Total.Should().Be(300m);
    }

    [Fact]
    public void Period_SplitsTodayAndMonth()
    {
        var monthStart = new DateTimeOffset(2026, 10, 23, 0, 0, 0, Iran).ToUniversalTime();
        var todayStart = new DateTimeOffset(2026, 10, 24, 0, 0, 0, Iran).ToUniversalTime();
        var tomorrowStart = new DateTimeOffset(2026, 10, 25, 0, 0, 0, Iran).ToUniversalTime();

        var payments = new List<Payment>
        {
            At(1, monthStart.AddHours(-1), 7m),
            At(1, monthStart.AddHours(1), 10m),
            At(1, todayStart.AddHours(1), 20m),
            At(1, tomorrowStart, 40m),
            At(2, todayStart.AddHours(1), 80m)
        };

        var period = PaymentQueries.Period(payments.AsQueryable(), 1, monthStart, todayStart, tomorrowStart).Single();

        period.MonthTotal.Should().Be(30m);
        period.MonthCount.Should().Be(2);
        period.TodayTotal.Should().Be(20m);
    }

    [Fact]
    public void ForStatement_OnlyThisCustomersReceivedPayments()
    {
        var payments = new List<Payment>
        {
            At(1, DateTimeOffset.UtcNow, 1m, customerId: 10),
            At(1, DateTimeOffset.UtcNow, 2m, customerId: 11),
            At(1, DateTimeOffset.UtcNow, 3m, customerId: 10, direction: TransactionDirection.Paid),
            At(2, DateTimeOffset.UtcNow, 4m, customerId: 10)
        };

        PaymentQueries.ForStatement(payments.AsQueryable(), 1, 10, default).ToList()
            .Should().ContainSingle().Which.Amount.Should().Be(1m);
    }
}
