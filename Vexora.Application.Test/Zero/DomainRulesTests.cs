using FluentAssertions;
using Vexora.Domain.Entities.Checks;
using Vexora.Domain.Entities.Customers;
using Vexora.Domain.Entities.Payments;
using Vexora.Domain.Enums.BankRegistrationStatuses;
using Vexora.Domain.Enums.CheckSettlementStatuses;
using Vexora.Domain.Exceptions.Checks;
using Vexora.Domain.Exceptions.Customers;
using Vexora.Domain.Exceptions.Subscriptions;
using Vexora.Domain.sharedKernel.Exceptions;
using Xunit;

namespace Vexora.Application.Test.Zero;

public sealed class DomainRulesTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 24, 9, 0, 0, TimeSpan.Zero);

    private static Check NewCheck()
        => Check.Create(1, 5, 20_000_000, new DateTime(2026, 12, 6), "123456");

    private static T Capture<T>(Action action) where T : Exception
    {
        // Assert.Throws خودش اگر استثنا پرتاب نشود، خطای استاندارد تست را پرتاب می‌کند
        return Xunit.Assert.Throws<T>(action);
    }

    [Fact]
    public void Customer_Should_Store_The_Normalized_Name()
    {
        var customer = Customer.Create(" علي ", "رضايي ", 1);

        customer.FirstName.Should().Be("علي");
        customer.NormalizedName.Should().Be("علی رضایی");
        customer.FullName.Should().Be("علي رضايي");
    }

    [Fact]
    public void Customer_Should_Update_The_Normalized_Name_When_Renamed()
    {
        var customer = Customer.Create("علی", "رضایی", 1);

        customer.ChangeName("علی", "رضایی بنا");

        customer.NormalizedName.Should().Be("علی رضایی بنا");
    }

    [Fact]
    public void Customer_Should_Allow_An_Empty_Last_Name()
    {
        Customer.Create("حاجی", null, 1).NormalizedName.Should().Be("حاجی");
    }

    [Fact]
    public void Customer_Should_Reject_Names_Longer_Than_The_Column()
    {
        var error = Capture<NameTooLongException>(
            () => Customer.Create(new string('ا', 81), "رضایی", 1));

        error.Code.Should().Be("CUSTOMER_NAME_TOO_LONG");
        error.Type.Should().Be(AppErrorType.Validation);
    }

    [Fact]
    public void Customer_Should_Reject_An_Empty_First_Name()
    {
        var action = () => Customer.Create("  ", "رضایی", 1);

        action.Should().Throw<FirstNameInvalidException>();
    }

    [Fact]
    public void Check_Should_Start_Pending_And_Not_Registered()
    {
        var check = NewCheck();

        check.SettlementStatus.Should().Be(CheckSettlementStatus.Pending);
        check.BankRegistrationStatus.Should().Be(BankRegistrationStatus.NotRegistered);
        check.PaidAt.Should().BeNull();
    }

    [Fact]
    public void Check_Should_Be_Paid_With_A_Date()
    {
        var check = NewCheck();

        check.MarkAsPaid(Now);

        check.SettlementStatus.Should().Be(CheckSettlementStatus.Paid);
        check.PaidAt.Should().Be(Now);
        check.BouncedAt.Should().BeNull();
    }

    [Fact]
    public void Check_Should_Be_Bounced_With_A_Date()
    {
        var check = NewCheck();

        check.MarkAsBounced(Now);

        check.SettlementStatus.Should().Be(CheckSettlementStatus.Bounced);
        check.BouncedAt.Should().Be(Now);
        check.PaidAt.Should().BeNull();
    }

    [Fact]
    public void Check_Should_Not_Be_Settled_Twice()
    {
        var check = NewCheck();
        check.MarkAsPaid(Now);

        var paidAgain = Capture<CheckNotPendingException>(() => check.MarkAsPaid(Now));
        var bounceAfterPaid = Capture<CheckNotPendingException>(() => check.MarkAsBounced(Now));

        paidAgain.Code.Should().Be("CHECK_NOT_PENDING");
        paidAgain.Type.Should().Be(AppErrorType.Conflict);
        bounceAfterPaid.Code.Should().Be("CHECK_NOT_PENDING");
    }

    [Fact]
    public void Check_Should_Go_Back_To_Pending_And_Clear_The_Dates()
    {
        var paid = NewCheck();
        paid.MarkAsPaid(Now);
        paid.RevertToPending();

        var bounced = NewCheck();
        bounced.MarkAsBounced(Now);
        bounced.RevertToPending();

        paid.SettlementStatus.Should().Be(CheckSettlementStatus.Pending);
        paid.PaidAt.Should().BeNull();
        bounced.SettlementStatus.Should().Be(CheckSettlementStatus.Pending);
        bounced.BouncedAt.Should().BeNull();
    }

    [Fact]
    public void Check_Should_Be_Settleable_Again_After_Revert()
    {
        var check = NewCheck();
        check.MarkAsPaid(Now);
        check.RevertToPending();

        check.MarkAsBounced(Now);

        check.SettlementStatus.Should().Be(CheckSettlementStatus.Bounced);
    }

    [Fact]
    public void Check_Should_Not_Revert_When_Already_Pending()
    {
        var error = Capture<CheckAlreadyPendingException>(() => NewCheck().RevertToPending());

        error.Code.Should().Be("CHECK_ALREADY_PENDING");
    }

    [Fact]
    public void Check_Bank_Registration_Should_Go_Both_Ways()
    {
        var check = NewCheck();

        check.SetBankRegistration(BankRegistrationStatus.Registered);
        check.BankRegistrationStatus.Should().Be(BankRegistrationStatus.Registered);

        check.SetBankRegistration(BankRegistrationStatus.NotRegistered);
        check.BankRegistrationStatus.Should().Be(BankRegistrationStatus.NotRegistered);
    }

    [Fact]
    public void Soft_Delete_Should_Hide_The_Record_And_Remember_When()
    {
        var payment = Payment.Create(1, 5, 500_000, Now);

        payment.SoftDelete();

        payment.IsDelete.Should().BeTrue();
        payment.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public void Soft_Delete_Twice_Should_Keep_The_First_Time()
    {
        var check = NewCheck();
        check.SoftDelete();
        var first = check.DeletedAt;

        check.SoftDelete();

        check.DeletedAt.Should().Be(first);
    }

    [Fact]
    public void Expired_Subscription_Error_Should_Be_Payment_Required()
    {
        var error = new SubscriptionExpiredException();

        error.Code.Should().Be("SUBSCRIPTION_EXPIRED");
        error.Type.Should().Be(AppErrorType.PaymentRequired);
    }

    [Fact]
    public void Duplicate_Customer_Error_Should_Be_A_Conflict()
    {
        var error = new CustomerAlreadyExistsException();

        error.Code.Should().Be("CUSTOMER_ALREADY_EXISTS");
        error.Type.Should().Be(AppErrorType.Conflict);
    }
}
