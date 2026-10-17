using Vexora.Domain.Entities.Customers;
using Vexora.Domain.Enums.BankRegistrationStatuses;
using Vexora.Domain.Enums.CheckSettlementStatuses;
using Vexora.Domain.Enums.TransactionDirections;
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

    public void MarkAsRegistered()
    {
        BankRegistrationStatus = BankRegistrationStatus.Registered;
    }

    public void MarkAsPaid()
    {
        if (SettlementStatus != CheckSettlementStatus.Pending)
            throw new InvalidOperationException();

        SettlementStatus = CheckSettlementStatus.Paid;
    }

    public void MarkAsBounced()
    {
        if (SettlementStatus != CheckSettlementStatus.Pending)
            throw new InvalidOperationException();

        SettlementStatus = CheckSettlementStatus.Bounced;
    }

    #endregion
}
