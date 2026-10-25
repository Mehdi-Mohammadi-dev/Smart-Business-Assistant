using FluentAssertions;
using Vexora.Domain.Entities.Checks;
using Vexora.Domain.Enums.BankRegistrationStatuses;
using Vexora.Domain.Enums.CheckSettlementStatuses;
using Vexora.Domain.Exceptions.Checks;
using Xunit;

namespace Vexora.Application.Test.Ledger;

public sealed class CheckDomainTests
{
    private static readonly DateTimeOffset At = new(2026, 10, 24, 9, 0, 0, TimeSpan.Zero);

    private static Check NewCheck()
        => Check.Create(1, 10, 5_000_000m, new DateTime(2026, 12, 6), "123456");

    [Fact]
    public void MarkAsPaid_FromPending_SetsStatusAndPaidAt()
    {
        var check = NewCheck();

        check.MarkAsPaid(At);

        check.SettlementStatus.Should().Be(CheckSettlementStatus.Paid);
        check.PaidAt.Should().Be(At);
        check.BouncedAt.Should().BeNull();
    }

    [Fact]
    public void MarkAsPaid_WhenNotPending_Throws()
    {
        var paid = NewCheck();
        paid.MarkAsPaid(At);

        var bounced = NewCheck();
        bounced.MarkAsBounced(At);

        paid.Invoking(x => x.MarkAsPaid(At))
            .Should().Throw<CheckExceptions.CheckNotPendingException>();

        bounced.Invoking(x => x.MarkAsPaid(At))
            .Should().Throw<CheckExceptions.CheckNotPendingException>();
    }

    [Fact]
    public void MarkAsBounced_FromPending_SetsStatusAndBouncedAt()
    {
        var check = NewCheck();

        check.MarkAsBounced(At);

        check.SettlementStatus.Should().Be(CheckSettlementStatus.Bounced);
        check.BouncedAt.Should().Be(At);
        check.PaidAt.Should().BeNull();
    }

    [Fact]
    public void MarkAsBounced_WhenNotPending_Throws()
    {
        var paid = NewCheck();
        paid.MarkAsPaid(At);

        var bounced = NewCheck();
        bounced.MarkAsBounced(At);

        paid.Invoking(x => x.MarkAsBounced(At))
            .Should().Throw<CheckExceptions.CheckNotPendingException>();

        bounced.Invoking(x => x.MarkAsBounced(At))
            .Should().Throw<CheckExceptions.CheckNotPendingException>();
    }

    [Fact]
    public void RevertToPending_FromPaid_ClearsPaidAt()
    {
        var check = NewCheck();
        check.MarkAsPaid(At);

        check.RevertToPending();

        check.SettlementStatus.Should().Be(CheckSettlementStatus.Pending);
        check.PaidAt.Should().BeNull();
        check.BouncedAt.Should().BeNull();
    }

    [Fact]
    public void RevertToPending_FromBounced_ClearsBouncedAt()
    {
        var check = NewCheck();
        check.MarkAsBounced(At);

        check.RevertToPending();

        check.SettlementStatus.Should().Be(CheckSettlementStatus.Pending);
        check.BouncedAt.Should().BeNull();
        check.PaidAt.Should().BeNull();
    }

    [Fact]
    public void RevertToPending_WhenAlreadyPending_Throws()
    {
        NewCheck().Invoking(x => x.RevertToPending())
            .Should().Throw<CheckExceptions.CheckAlreadyPendingException>();
    }

    [Fact]
    public void SetBankRegistration_CanGoBothWays()
    {
        var check = NewCheck();

        check.SetBankRegistration(BankRegistrationStatus.Registered);
        check.BankRegistrationStatus.Should().Be(BankRegistrationStatus.Registered);

        check.SetBankRegistration(BankRegistrationStatus.NotRegistered);
        check.BankRegistrationStatus.Should().Be(BankRegistrationStatus.NotRegistered);
    }

    [Fact]
    public void SoftDelete_MarksDeletedOnce()
    {
        var check = NewCheck();

        check.SoftDelete();
        var firstDeletedAt = check.DeletedAt;

        check.SoftDelete();

        check.IsDelete.Should().BeTrue();
        firstDeletedAt.Should().NotBeNull();
        check.DeletedAt.Should().Be(firstDeletedAt);
    }
}
