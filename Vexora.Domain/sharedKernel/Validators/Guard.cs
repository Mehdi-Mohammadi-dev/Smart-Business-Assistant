using Vexora.Domain.sharedKernel.Exceptions;

namespace Vexora.Domain.sharedKernel.Validators;

public static class Guard
{
    public static void AgainstInvalidId(long id)
    {
        if (id <= 0)
            throw new InvalidIdException();
    }

    public static void AmountValidations(decimal amount)
    {
        if (amount <= 0)
        {
            throw new AmountInvalidException();
        }
    }
}
