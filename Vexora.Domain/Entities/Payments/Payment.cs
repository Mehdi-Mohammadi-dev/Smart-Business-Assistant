using Vexora.Domain.Entities.Customers;
using Vexora.Domain.Enums.TransactionDirections;
using Vexora.Domain.sharedKernel.BaseEntities;
using Vexora.Domain.sharedKernel.Validators;

namespace Vexora.Domain.Entities.Payments;

public sealed class Payment : UserBaseEntity
{
    public long CustomerId { get; private set; }

    public decimal Amount { get; private set; }

    public DateTimeOffset PaymentDate { get; private set; }

    public TransactionDirection Direction { get; private set; }

    #region Relations

    public Customer Customer { get; private set; } = null!;

    #endregion

    #region Constractors

    private Payment() { }

    #endregion

    #region Functions

    public static Payment Create(
        long userId,
        long customerId,
        decimal amount,
        DateTimeOffset paymentDate,
        TransactionDirection direction=TransactionDirection.Received)
    {
        Guard.AgainstInvalidId(userId);
        Guard.AgainstInvalidId(customerId);
        Guard.AmountValidations(amount);

        return new Payment
        {
            UserId = userId,
            CustomerId = customerId,
            Amount = amount,
            PaymentDate = paymentDate,
            Direction = direction,
            CreateAt = DateTimeOffset.UtcNow,
            UpdateAt = DateTimeOffset.UtcNow
        };
    }

    #endregion
}