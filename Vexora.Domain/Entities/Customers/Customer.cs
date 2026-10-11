using Vexora.Domain.sharedKernel.BaseEntities;
using Vexora.Domain.sharedKernel.Validators;
using Vexora.Domain.Validators.Customers;

namespace Vexora.Domain.Entities.Customers;

public sealed class Customer:UserBaseEntity
{
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; }= string.Empty;

    #region Constractor

    private Customer()
    {
        
    }

    #endregion

    #region Functions

    public static Customer Create(string  firstName, string? lastName,long userId)
    {
        Guard.AgainstInvalidId(userId);
        firstName = firstName.Trim();
        lastName = lastName?.Trim() ?? string.Empty;
        CustomerValidations.ValidatName(firstName);
        
        return new Customer
        {
            UserId=userId,
            FirstName = firstName,
            LastName = lastName,
            CreateAt = DateTimeOffset.Now,
            UpdateAt = DateTimeOffset.Now
        };
    }

    public void ChangeName(string firstName, string? lastName)
    {
        firstName = firstName.Trim();
        lastName = lastName?.Trim() ?? string.Empty;
        CustomerValidations.ValidatName(firstName);
        FirstName = firstName;
        LastName = lastName;
        
    }

    #endregion
}
