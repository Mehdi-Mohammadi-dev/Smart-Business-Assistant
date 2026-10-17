using Vexora.Domain.Exceptions.Customers;

namespace Vexora.Domain.Validators.Customers;

public static class CustomerValidations
{
    public static void ValidatName(string firstName)
    {
        if(string.IsNullOrWhiteSpace(firstName))
            throw new CustomerExceptions.FirstNameInvalidException();
    }
}
