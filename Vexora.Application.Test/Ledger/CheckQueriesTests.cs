using FluentAssertions;
using Vexora.Application.Ledger.Checks;
using Vexora.Application.Ledger.Queries;
using Vexora.Domain.Entities.Checks;
using Vexora.Domain.Enums.BankRegistrationStatuses;
using Vexora.Domain.Enums.CheckSettlementStatuses;
using Vexora.Domain.Enums.TransactionDirections;
using Xunit;

namespace Vexora.Application.Test.Ledger;

public sealed class CheckQueriesTests
{
    private static readonly DateTime Today = new(2026, 10, 24);
    private static readonly DateTimeOffset Now = new(2026, 10, 24, 9, 0, 0, TimeSpan.Zero);

    private static Check Pending(long userId, int dueOffsetDays, long customerId = 10)
        => Check.Create(userId, customerId, 1_000_000m, Today.AddDays(dueOffsetDays), "1" + dueOffsetDays);

    private static CheckListQuery Query(
        bool overdue = false,
        int? within = null,
        CheckSettlementStatus? settlement = null,
        DateTime? dueFrom = null,
        DateTime? dueTo = null,
        long? customerId = null)
        => new(customerId, settlement, null, dueFrom, dueTo, overdue, within, null, CheckSort.DueDate, 1, 20);

    private static List<int> DueOffsets(IEnumerable<Check> checks)
        => checks.Select(x => (int)(x.DueDate - Today).TotalDays).OrderBy(x => x).ToList();

    private static List<Check> Sample()
    {
        var paid = Pending(1, -5);
        paid.MarkAsPaid(Now);

        return
        [
            Pending(1, -1),
            Pending(1, 0),
            Pending(1, 1),
            Pending(1, 7),
            Pending(1, 8),
            paid,
            Pending(2, -3),
            Pending(2, 2)
        ];
    }

    [Fact]
    public void Overdue_ReturnsOnlyPendingBeforeToday()
    {
        var result = CheckQueries.Filter(Sample().AsQueryable(), 1, Query(overdue: true), Today).ToList();

        DueOffsets(result).Should().Equal(-1);
    }

    [Theory]
    [InlineData(0, new[] { 0 })]
    [InlineData(1, new[] { 0, 1 })]
    [InlineData(7, new[] { 0, 1, 7 })]
    public void DueWithinDays_IncludesTodayAndDayN_ExcludesDayAfterAndPast(int days, int[] expected)
    {
        var result = CheckQueries.Filter(Sample().AsQueryable(), 1, Query(within: days), Today).ToList();

        DueOffsets(result).Should().Equal(expected);
    }

    [Fact]
    public void Filters_AreCombinedWithAnd()
    {
        var result = CheckQueries.Filter(
            Sample().AsQueryable(),
            1,
            Query(overdue: true, settlement: CheckSettlementStatus.Paid),
            Today).ToList();

        result.Should().BeEmpty();
    }

    [Fact]
    public void DueRange_IsInclusiveOnBothEnds()
    {
        var result = CheckQueries.Filter(
            Sample().AsQueryable(),
            1,
            Query(dueFrom: Today, dueTo: Today.AddDays(1)),
            Today).ToList();

        DueOffsets(result).Should().Equal(0, 1);
    }

    [Fact]
    public void Settlement_FiltersByStatus()
    {
        var result = CheckQueries.Filter(
            Sample().AsQueryable(),
            1,
            Query(settlement: CheckSettlementStatus.Paid),
            Today).ToList();

        result.Should().ContainSingle();
    }

    [Fact]
    public void Filter_NeverReturnsOtherUsersChecks()
    {
        var one = CheckQueries.Filter(Sample().AsQueryable(), 1, Query(), Today).ToList();
        var two = CheckQueries.Filter(Sample().AsQueryable(), 2, Query(), Today).ToList();

        one.Should().OnlyContain(x => x.UserId == 1);
        two.Should().OnlyContain(x => x.UserId == 2);
        two.Should().HaveCount(2);
    }

    [Fact]
    public void Sort_DueDateAscending()
    {
        var sorted = CheckQueries.Sort(
            CheckQueries.Filter(Sample().AsQueryable(), 1, Query(), Today),
            CheckSort.DueDate).ToList();

        sorted.Select(x => x.DueDate).Should().BeInAscendingOrder();
    }

    [Fact]
    public void ByCustomer_SumsOnlyReceivedAndSplitsByStatus()
    {
        var paid = Pending(1, -5);
        paid.MarkAsPaid(Now);

        var paidDirection = Check.Create(1, 10, 9_000_000m, Today.AddDays(3), "9", TransactionDirection.Paid);

        var checks = new List<Check>
        {
            Pending(1, -2),
            Pending(1, 4),
            paid,
            paidDirection,
            Pending(2, 1)
        };

        var aggregate = CheckQueries.ByCustomer(checks.AsQueryable(), 1, null, Today).Single();

        aggregate.CustomerId.Should().Be(10);
        aggregate.PendingCount.Should().Be(2);
        aggregate.PendingTotal.Should().Be(2_000_000m);
        aggregate.OverdueCount.Should().Be(1);
        aggregate.PaidTotal.Should().Be(1_000_000m);
    }

    [Fact]
    public void PendingSummary_CountsOverdueDueSoonAndNotRegistered()
    {
        var registered = Check.Create(
            1, 10, 2_000_000m, Today.AddDays(2), "r",
            TransactionDirection.Received, BankRegistrationStatus.Registered);

        var checks = new List<Check>
        {
            Pending(1, -1),
            Pending(1, 3),
            Pending(1, 30),
            registered,
            Pending(2, 1)
        };

        var summary = CheckQueries.PendingSummary(checks.AsQueryable(), 1, Today, 7).Single();

        summary.PendingCount.Should().Be(4);
        summary.PendingTotal.Should().Be(5_000_000m);
        summary.OverdueCount.Should().Be(1);
        summary.OverdueTotal.Should().Be(1_000_000m);
        summary.DueSoonCount.Should().Be(2);
        summary.DueSoonTotal.Should().Be(3_000_000m);
        summary.NotRegisteredCount.Should().Be(3);
    }

    [Fact]
    public void ForStatement_FiltersByDueDateRangeAndCustomer()
    {
        var checks = new List<Check>
        {
            Pending(1, -10),
            Pending(1, 0),
            Pending(1, 5),
            Pending(1, 0, customerId: 99)
        };

        var rows = CheckQueries.ForStatement(
            checks.AsQueryable(), 1, 10, Today.AddDays(-1), Today.AddDays(5)).ToList();

        rows.Select(x => (int)(x.DueDate - Today).TotalDays).Should().BeEquivalentTo(new[] { 0, 5 });
    }
}
