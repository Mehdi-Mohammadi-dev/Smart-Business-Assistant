using Vexora.Domain.Constants.Customers;

namespace Vexora.Domain.Exceptions.Customers;

public static class CustomerExceptions
{
    public sealed class FirstNameInvalidException() : Exception
        (CustomerConstants.FirstNameInvalid);

    public sealed class LastNameInvalidException() : Exception
    (CustomerConstants.LastNameInvalid);
}
