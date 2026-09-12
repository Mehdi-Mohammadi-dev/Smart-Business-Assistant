using Vexora.Domain.Exceptions.Customers;

namespace Vexora.Domain.Validators.Customers;

public static class CustomerValidations
{
    public static void FirstNameAndLastnameValidator(string firstName,string lastname)
    { 
        if(string.IsNullOrWhiteSpace(firstName))
            throw new CustomerExceptions.FirstNameInvalidException();

        if (string.IsNullOrWhiteSpace(lastname))
            throw new CustomerExceptions.LastNameInvalidException();
    }
}
