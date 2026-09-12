using Vexora.Domain.Entities.Customers;
using Vexora.Domain.sharedKernel.BaseEntities;
using Vexora.Domain.sharedKernel.Validators;
using Vexora.Domain.Validators.Payments;

namespace Vexora.Domain.Entities.Checks;

public sealed class Check : UserBaseEntity
{
    public long CustomerId { get; private set; }

    public decimal Amount { get; private set; }

    public DateTime DueDate { get; private set; }

    public string? CheckNumber { get; private set; }

    #region Constractor

    private Check()
    {
    }

    #endregion

    #region Relations

    public Customer Customer { get; private set; } = null!;

    #endregion

    #region Functions

    public static Check Create(long userId,long customerId, decimal amount, DateTime dueDate, string? checkNumber = null)
    {
        Guard.AgainstNegativeId(customerId);
        Guard.AgainstNegativeId(userId);

        PaymentValidations.AmountValidations(amount);

        return new Check
        {
            UserId = userId,
            CustomerId = customerId,
            Amount = amount,
            DueDate = dueDate,
            CheckNumber = checkNumber,
            CreateAt = DateTimeOffset.UtcNow,
            UpdateAt = DateTimeOffset.UtcNow
        };
    }

    #endregion
}
