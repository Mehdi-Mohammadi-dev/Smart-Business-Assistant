using System;
using System.Collections.Generic;
using System.Text;
using Vexora.Domain.Exceptions.Payments;

namespace Vexora.Domain.Validators.Payments;

public static class PaymentValidations
{
    public static void AmountValidations(decimal amount)
    {
        if (amount < 0) 
        {
            throw new PaymentExceptions.AmountInvaidException();
        }
    }
}
