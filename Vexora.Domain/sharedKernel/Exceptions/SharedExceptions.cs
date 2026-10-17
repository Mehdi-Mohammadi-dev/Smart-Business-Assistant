using Vexora.Domain.sharedKernel.Constants;

namespace Vexora.Domain.sharedKernel.Exceptions;

public static class SharedExceptions
{
    public sealed class InvalidIdException() : Exception
        (Messages.IdInvalid);

    public sealed class AmountInvalidException() : Exception
    (Messages.AmountInvalid);
}
