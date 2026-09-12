using System;
using System.Collections.Generic;
using System.Text;
using static Vexora.Domain.sharedKernel.Exceptions.SharedExceptions;

namespace Vexora.Domain.sharedKernel.Validators;

public static class Guard
{
    public static void AgainstNegativeId(long id)
    {
        if (id == 0)
            throw new InvalidIdException();
    }
}
