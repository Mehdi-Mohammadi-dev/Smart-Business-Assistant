using System;
using System.Collections.Generic;
using System.Text;
using Vexora.Domain.Constants.Sales.Items;

namespace Vexora.Domain.Exceptions.Sales.Items;

public sealed class QuantityException() : Exception
(SaleItemConstants.QuantityInvalid);
