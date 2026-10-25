using Vexora.Domain.Exceptions.Sales.Items;

namespace Vexora.Domain.Validators.Sales.Items;

public static class SaleItemValidations
{
    public static void QuantityValidator(decimal quantity)
    {
        if (quantity <= 0)
            throw new QuantityException();
    }

    public static void UnitPriceValidator(decimal unitPrice)
    {
        if (unitPrice < 0)
            throw new UnitPriceException();
    }
}
