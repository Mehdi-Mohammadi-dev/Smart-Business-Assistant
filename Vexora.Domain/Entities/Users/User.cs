using Vexora.Domain.Enums.UserRoles;
using Vexora.Domain.sharedKernel.BaseEntities;
using Vexora.Domain.Validators.Users;

namespace Vexora.Domain.Entities.Users;

public class User : BaseEntity
{
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public string PhoneNumber { get; private set; } = null!;
    public string PasswordHasher { get; private set; } = null!;
    public UserRole Role { get; private set; } = UserRole.User;

    #region Constractor

    private User()
    {
    }

    #endregion

    #region Functions

    public static User Register(string firstName, string lastName, string phoneNumber, string passwordHasher)
    {
        UserValidations.ValidatName(firstName, lastName);

        UserValidations.PhoneNumberValidation(phoneNumber);

        UserValidations.PasswordValidation(passwordHasher);

        return new User
        {
            FirstName = firstName,
            LastName = lastName,
            PhoneNumber = phoneNumber,
            PasswordHasher = passwordHasher,
            CreateAt = DateTime.UtcNow,
            UpdateAt = DateTime.UtcNow,
        };
    }

    public static User CreateSystemAdmin(string phoneNumber, string passwordHash)
    {
        UserValidations.PhoneNumberValidation(phoneNumber);
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash is required.", nameof(passwordHash));
        return new User
        {
            FirstName = "System",
            LastName = "Admin",
            PhoneNumber = phoneNumber,
            PasswordHasher = passwordHash,
            Role = UserRole.SystemAdmin,
            CreateAt = DateTimeOffset.UtcNow,
            UpdateAt = DateTimeOffset.UtcNow
        };
    }

    public void UpdatePassword(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash is required.", nameof(passwordHash));
        PasswordHasher = passwordHash;
        UpdateAt = DateTimeOffset.UtcNow;
    }

    public void Update(string firstName, string lastName, string phoneNumber, string passwordHasher)
    {
        UserValidations.ValidatName(firstName, lastName);

        UserValidations.PhoneNumberValidation(phoneNumber);

        UserValidations.PasswordValidation(passwordHasher);

        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
        PasswordHasher = passwordHasher;
    }

    #endregion
}
