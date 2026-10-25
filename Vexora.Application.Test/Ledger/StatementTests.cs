using FluentAssertions;
using Moq;
using Vexora.Application.Ledger.Exceptions;
using Vexora.Application.Ledger.Models;
using Vexora.Application.Ledger.Repositories;
using Vexora.Application.Ledger.Statements;
using Vexora.Application.Services.Interfaces.CurrentUser;
using Vexora.Domain.Enums.BankRegistrationStatuses;
using Vexora.Domain.Enums.CheckSettlementStatuses;
using Xunit;

using Vexora.Application.Test.Ledger.StatementTestsHelpers;

namespace Vexora.Application.Test.Ledger;

public sealed class StatementTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 24, 9, 0, 0, TimeSpan.Zero);

    private static StatementPaymentRow Payment(long id, decimal amount, DateTimeOffset date)
        => new() { Id = id, Amount = amount, PaymentDate = date };

    private static StatementCheckRow Check(
        long id, decimal amount, CheckSettlementStatus status, DateTimeOffset createdAt, int dueOffset = 10)
        => new()
        {
            Id = id,
            Amount = amount,
            SettlementStatus = status,
            CreatedAt = createdAt,
            DueDate = new DateTime(2026, 10, 24).AddDays(dueOffset),
            CheckNumber = "12" + id,
            BankRegistration = BankRegistrationStatus.NotRegistered
        };

    [Fact]
    public void Totals_PendingAndBouncedAreNotInTotalReceived()
    {
        var totals = StatementBuilder.BuildTotals(
            [Payment(1, 1_000_000m, Now), Payment(2, 500_000m, Now)],
            [
                Check(1, 2_000_000m, CheckSettlementStatus.Pending, Now),
                Check(2, 3_000_000m, CheckSettlementStatus.Paid, Now),
                Check(3, 700_000m, CheckSettlementStatus.Bounced, Now)
            ]);

        totals.CashReceived.Should().Be(1_500_000m);
        totals.ChecksPending.Should().Be(2_000_000m);
        totals.ChecksPaid.Should().Be(3_000_000m);
        totals.ChecksBounced.Should().Be(700_000m);
        totals.TotalReceived.Should().Be(4_500_000m);
        totals.PaymentsCount.Should().Be(2);
        totals.ChecksCount.Should().Be(3);
    }

    [Fact]
    public void Rows_AreOrderedNewestFirst_AndMapped()
    {
        var statement = StatementBuilder.Build(
            5,
            "علی رضایی",
            null,
            null,
            [Payment(1, 100m, Now.AddDays(-3)), Payment(2, 200m, Now.AddDays(-1))],
            [Check(7, 300m, CheckSettlementStatus.Pending, Now.AddDays(-2))],
            Now);

        statement.Rows.Select(x => (x.Type, x.Id)).Should().Equal(
            ("Payment", 2L), ("Check", 7L), ("Payment", 1L));

        var check = statement.Rows[1];
        check.Status.Should().Be("Pending");
        check.BankRegistration.Should().Be("NotRegistered");
        check.CheckNumber.Should().Be("127");
        check.DueDate.Should().Be(new DateTime(2026, 11, 3));
        check.DueDatePersian.Should().Be("1405/08/12");

        var payment = statement.Rows[0];
        payment.Status.Should().BeNull();
        payment.DueDate.Should().BeNull();
        payment.BankRegistration.Should().BeNull();
        payment.DatePersian.Should().Be("1405/08/01");
    }


    private static StatementService Create(FakeStatementReader reader, long userId = 1)
    {
        var currentUser = new Mock<ICurrentUser>();
        currentUser.Setup(x => x.UserId).Returns(userId);

        return new StatementService(reader, currentUser.Object, new LedgerFixedClock(Now));
    }

    [Fact]
    public async Task Get_MoreThanLimit_Throws()
    {
        var act = () => Create(new FakeStatementReader(StatementService.MaxRows + 1)).GetAsync(5, null, null);

        await act.Should().ThrowAsync<StatementTooLargeException>();
    }

    [Fact]
    public async Task Get_ExactlyLimit_IsAllowed()
    {
        var statement = await Create(new FakeStatementReader(StatementService.MaxRows)).GetAsync(5, null, null);

        statement.CustomerName.Should().Be("علی رضایی");
        statement.GeneratedAt.Should().Be(Now);
    }

    [Fact]
    public async Task Get_FromAfterTo_Throws()
    {
        var act = () => Create(new FakeStatementReader(0))
            .GetAsync(5, new DateTime(2026, 10, 25), new DateTime(2026, 10, 24));

        await act.Should().ThrowAsync<InvalidDateRangeException>();
    }

    [Fact]
    public async Task Get_UnknownCustomer_Throws()
    {
        var act = () => Create(new FakeStatementReader(0, name: null)).GetAsync(5, null, null);

        await act.Should().ThrowAsync<CustomerNotFoundException>();
    }

    [Fact]
    public async Task Get_UsesCurrentUsersId()
    {
        var reader = new FakeStatementReader(0);

        await Create(reader, userId: 42).GetAsync(5, null, null);

        reader.SeenUserId.Should().Be(42);
    }
}
