using Vexora.Domain.sharedKernel.BaseEntities;
using Vexora.Domain.sharedKernel.Validators;
using Vexora.Domain.Validators.Sales.Items;

namespace Vexora.Domain.Entities.Sales.Items;

public sealed class SaleItem : BaseEntity
{
    public string? Description { get; private set; }

    public decimal Quantity { get; private set; }

    public decimal UnitPrice { get; private set; }

    public long SaleId { get; private set; }

    #region Constractor

    private SaleItem()
    {
    }

    #endregion

    #region Functions

    internal static SaleItem Create(long saleId,string description,decimal quantity,decimal unitPrice)
    {
        Guard.AgainstInvalidId(saleId);
        SaleItemValidations.QuantityValidator(quantity);
        SaleItemValidations.UnitPriceValidator(unitPrice);

        return new SaleItem
        {
            SaleId=saleId,
           Description= description.Trim(),
           Quantity= quantity,
           UnitPrice= unitPrice
        };
    }

    #endregion
}
