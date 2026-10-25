using System.ComponentModel.DataAnnotations.Schema;
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

    // Derived in memory to avoid requiring a database migration for this recovery fix.
    [NotMapped]
    public string NormalizedName => CustomerNameNormalizer.Normalize(FirstName, LastName);

    [NotMapped]
    public string FullName => $"{FirstName} {LastName}".Trim();

    private Customer()
    {
    }

    public static Customer Create(string firstName, string? lastName, long userId)
    {
        Guard.AgainstInvalidId(userId);
        firstName = firstName.Trim();
        lastName = lastName?.Trim() ?? string.Empty;

        CustomerValidations.ValidatName(firstName);
        ValidateNameLength(firstName, lastName);

        var now = DateTimeOffset.UtcNow;
        return new Customer
        {
            UserId = userId,
            FirstName = firstName,
            LastName = lastName,
            CreateAt = now,
            UpdateAt = now
        };
    }

    public void ChangeName(string firstName, string? lastName)
    {
        firstName = firstName.Trim();
        lastName = lastName?.Trim() ?? string.Empty;

        CustomerValidations.ValidatName(firstName);
        ValidateNameLength(firstName, lastName);

        FirstName = firstName;
        LastName = lastName;
        UpdateAt = DateTimeOffset.UtcNow;
    }

    private static void ValidateNameLength(string firstName, string lastName)
    {
        if (firstName.Length > CustomerConstants.NameLength ||
            lastName.Length > CustomerConstants.NameLength)
        {
            throw new CustomerExceptions.NameTooLongException();
        }
    }
}
