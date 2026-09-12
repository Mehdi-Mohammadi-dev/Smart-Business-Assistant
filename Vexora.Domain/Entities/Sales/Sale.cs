using Vexora.Domain.Entities.Customers;
using Vexora.Domain.Entities.Sales.Items;
using Vexora.Domain.sharedKernel.BaseEntities;
using Vexora.Domain.sharedKernel.Validators;

namespace Vexora.Domain.Entities.Sales;

public sealed class Sale : UserBaseEntity
{
    public long CustomerId { get; private set; }

    public DateTime SaleDate { get; private set; }

    public IReadOnlyCollection<SaleItem> Items =>
        _items.AsReadOnly();

    #region Relations

    private readonly List<SaleItem> _items = [];
    public Customer Customer { get; private set; } = null!;

    #endregion

    #region Constractor

    private Sale()
    {
    }

    #endregion

    #region Functions

    public static Sale Create(long userId,long customerId,DateTime saleDate)
    {
        Guard.AgainstNegativeId(customerId);
        Guard.AgainstNegativeId(userId);

        return new Sale
        {
            UserId = userId,
            CustomerId = customerId,
            SaleDate = saleDate
        };

    }
    public void AddItem(long saleId,string description, decimal quantity, decimal unitPrice)
    {
        _items.Add(SaleItem.Create(saleId,description, quantity, unitPrice));
    }

    #endregion

}
