using System;
using System.Collections.Generic;
using System.Text;
using Vexora.Domain.Constants.Sales.Items;

namespace Vexora.Domain.Exceptions.Sales.Items;

public static class SaleItemExceptions
{
    public sealed class QuantityException() : Exception
        (SaleItemConstants.QuantityInvalid);

    public sealed class UnitPriceException() : Exception
    (SaleItemConstants.UnitPriceInvalid);
}
