using Vexora.Domain.Entities.Customers;
using Vexora.Domain.sharedKernel.BaseEntities;
using Vexora.Domain.sharedKernel.Validators;
using Vexora.Domain.Validators.Payments;

namespace Vexora.Domain.Entities.Payments;

public sealed class Payment :UserBaseEntity
{
    public long CustomerId { get; private set; }

    public decimal Amount { get; private set; }

    public DateTime PaymentDate { get; private set; }

    #region Relations

    public Customer Customer { get; private set; } = null!;

    #endregion

    #region Constractor

    private Payment()
    {
    }

    #endregion

    #region Functions

    public static Payment Create(long userId,long customerId,decimal amount,DateTime paymentDate)
    {
        Guard.AgainstNegativeId(customerId);
        Guard.AgainstNegativeId(userId);

        PaymentValidations.AmountValidations(amount);

        return new Payment 
        {
            UserId = userId,
            CustomerId= customerId,
            Amount= amount,
            PaymentDate= paymentDate
        };
    }

    #endregion
}
