using Vexora.Domain.Entities.Customers;
using Vexora.Domain.Enums.BankRegistrationStatuses;
using Vexora.Domain.Enums.CheckSettlementStatuses;
using Vexora.Domain.Enums.TransactionDirections;
using Vexora.Domain.Exceptions.Checks;
using Vexora.Domain.sharedKernel.BaseEntities;
using Vexora.Domain.sharedKernel.Validators;

namespace Vexora.Domain.Entities.Checks;

public sealed class Check : UserBaseEntity
{
    public long CustomerId { get; private set; }

    public decimal Amount { get; private set; }

    public DateTime DueDate { get; private set; }

    public string? CheckNumber { get; private set; }

    public TransactionDirection Direction { get; private set; }

    public BankRegistrationStatus BankRegistrationStatus { get; private set; }

    public CheckSettlementStatus SettlementStatus { get; private set; }

    public DateTimeOffset? PaidAt { get; private set; }

    public DateTimeOffset? BouncedAt { get; private set; }

    #region Constractor

    private Check()
    {
    }

    #endregion

    #region Relations

    public Customer Customer { get; private set; } = null!;

    #endregion

    #region Functions

    public static Check Create(long userId,long customerId, decimal amount, DateTime dueDate, string? checkNumber = null,
        TransactionDirection direction = TransactionDirection.Received,
        BankRegistrationStatus bankRegistrationStatus = BankRegistrationStatus.NotRegistered)
    {
        Guard.AgainstInvalidId(customerId);
        Guard.AgainstInvalidId(userId);

        Guard.AmountValidations(amount);

        return new Check
        {
            UserId = userId,
            CustomerId = customerId,
            Amount = amount,
            DueDate = dueDate,
            CheckNumber = checkNumber,
            Direction = direction,
            BankRegistrationStatus = bankRegistrationStatus,
            SettlementStatus = CheckSettlementStatus.Pending,
            CreateAt = DateTimeOffset.UtcNow,
            UpdateAt = DateTimeOffset.UtcNow
        };
    }

    public void SetBankRegistration(BankRegistrationStatus status)
    {
        BankRegistrationStatus = status;
        UpdateAt = DateTimeOffset.UtcNow;
    }

    public void MarkAsPaid(DateTimeOffset paidAt)
    {
        if (SettlementStatus != CheckSettlementStatus.Pending)
            throw new CheckExceptions.CheckNotPendingException();

        SettlementStatus = CheckSettlementStatus.Paid;
        PaidAt = paidAt;
        UpdateAt = DateTimeOffset.UtcNow;
    }

    public void MarkAsBounced(DateTimeOffset bouncedAt)
    {
        if (SettlementStatus != CheckSettlementStatus.Pending)
            throw new CheckExceptions.CheckNotPendingException();

        SettlementStatus = CheckSettlementStatus.Bounced;
        BouncedAt = bouncedAt;
        UpdateAt = DateTimeOffset.UtcNow;
    }

    public void RevertToPending()
    {
        if (SettlementStatus == CheckSettlementStatus.Pending)
            throw new CheckExceptions.CheckAlreadyPendingException();

        SettlementStatus = CheckSettlementStatus.Pending;
        PaidAt = null;
        BouncedAt = null;
        UpdateAt = DateTimeOffset.UtcNow;
    }

    #endregion
}
