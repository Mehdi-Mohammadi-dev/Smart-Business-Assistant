using FluentAssertions;
using Vexora.Application.Common;
using Vexora.Application.Ledger.Checks;
using Vexora.Domain.Enums.CheckSettlementStatuses;
using Xunit;

namespace Vexora.Application.Test.Ledger;

public sealed class CheckRulesTests
{
    private static readonly DateTime Today = new(2026, 10, 24);

    [Fact]
    public void DaysToDue_Pending_CountsDaysUntilDueDate()
    {
        CheckRules.DaysToDue(CheckSettlementStatus.Pending, new DateTime(2026, 12, 6), Today)
            .Should().Be(43);
    }

    [Fact]
    public void DaysToDue_PastDueDate_IsNegative()
    {
        CheckRules.DaysToDue(CheckSettlementStatus.Pending, new DateTime(2026, 10, 23), Today)
            .Should().Be(-1);
    }

    [Theory]
    [InlineData(CheckSettlementStatus.Paid)]
    [InlineData(CheckSettlementStatus.Bounced)]
    public void DaysToDue_NotPending_IsNull(CheckSettlementStatus status)
    {
        CheckRules.DaysToDue(status, new DateTime(2026, 10, 1), Today)
            .Should().BeNull();
    }

    [Theory]
    [InlineData(-1, true)]
    [InlineData(0, false)]
    [InlineData(5, false)]
    public void IsOverdue_OnlyWhenDaysToDueIsNegative(int days, bool expected)
    {
        CheckRules.IsOverdue(days).Should().Be(expected);
    }

    [Fact]
    public void IsOverdue_NullDays_IsFalse()
    {
        CheckRules.IsOverdue(null).Should().BeFalse();
    }

    [Fact]
    public void IranMidnight_FlipsDueTodayIntoOverdue()
    {
        var dueDate = new DateTime(2026, 10, 24);

        var beforeMidnight = IranTime.Today(
            new LedgerFixedClock(new DateTimeOffset(2026, 10, 24, 20, 29, 59, TimeSpan.Zero)));

        var afterMidnight = IranTime.Today(
            new LedgerFixedClock(new DateTimeOffset(2026, 10, 24, 20, 30, 0, TimeSpan.Zero)));

        var before = CheckRules.DaysToDue(CheckSettlementStatus.Pending, dueDate, beforeMidnight);
        var after = CheckRules.DaysToDue(CheckSettlementStatus.Pending, dueDate, afterMidnight);

        before.Should().Be(0);
        CheckRules.IsOverdue(before).Should().BeFalse();

        after.Should().Be(-1);
        CheckRules.IsOverdue(after).Should().BeTrue();
    }
}
