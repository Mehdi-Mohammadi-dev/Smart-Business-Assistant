using Vexora.Domain.sharedKernel.BaseEntities;
using Vexora.Domain.sharedKernel.Validators;
using Vexora.Domain.Validators.Customers;

namespace Vexora.Domain.Entities.Customers;

public sealed class Customer:UserBaseEntity
{
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; }= null!;

    #region Constractor

    private Customer()
    {
        
    }

    #endregion

    #region Functions

    public static Customer Create(long userId,string  firstName, string lastName)
    {
        Guard.AgainstNegativeId(userId);

        CustomerValidations.FirstNameAndLastnameValidator(firstName, lastName);
        
        return new Customer
        {
            UserId = userId,
            FirstName = firstName,
            LastName = lastName,
            CreateAt = DateTime.Now,
            UpdateAt = DateTime.Now
        };
    }

    public void ChangeName(string firstName, string lastName)
    {
        CustomerValidations.FirstNameAndLastnameValidator(firstName, lastName);
        FirstName = firstName;
        LastName = lastName;
        Update();
    }

    #endregion
}
