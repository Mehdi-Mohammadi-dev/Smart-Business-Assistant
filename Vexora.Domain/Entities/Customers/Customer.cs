using Vexora.Domain.Constants.Customers;
using Vexora.Domain.Exceptions.Customers;
using Vexora.Domain.sharedKernel.BaseEntities;
using Vexora.Domain.sharedKernel.Validators;
using Vexora.Domain.Validators.Customers;

namespace Vexora.Domain.Entities.Customers;

public sealed class Customer : UserBaseEntity
{
    public string FirstName { get; private set; } = null!;

    public string LastName { get; private set; } = string.Empty;

    public string NormalizedName { get; private set; } = string.Empty;

    public string FullName => $"{FirstName} {LastName}".Trim();

    #region Constractor
    private Customer()
    {
    }
    #endregion

    #region Functions
    public static Customer Create(string firstName, string? lastName, long userId)
    {
        Guard.AgainstInvalidId(userId);

        firstName = firstName?.Trim() ?? string.Empty;
        lastName = lastName?.Trim() ?? string.Empty;

        ValidateNames(firstName, lastName);

        var now = DateTimeOffset.UtcNow;

        return new Customer
        {
            UserId = userId,
            FirstName = firstName,
            LastName = lastName,
            NormalizedName = CustomerNameNormalizer.Normalize(firstName, lastName),
            CreateAt = now,
            UpdateAt = now
        };
    }

    public void ChangeName(string firstName, string? lastName)
    {
        firstName = firstName?.Trim() ?? string.Empty;
        lastName = lastName?.Trim() ?? string.Empty;

        ValidateNames(firstName, lastName);

        FirstName = firstName;
        LastName = lastName;
        NormalizedName = CustomerNameNormalizer.Normalize(firstName, lastName);
        UpdateAt = DateTimeOffset.UtcNow;
    }

    private static void ValidateNames(string firstName, string lastName)
    {
        CustomerValidations.ValidatName(firstName);

        if (firstName.Length > CustomerConstants.NameLength ||
            lastName.Length > CustomerConstants.NameLength)
        {
            throw new CustomerExceptions.NameTooLongException();
        }
    }
    #endregion
}
