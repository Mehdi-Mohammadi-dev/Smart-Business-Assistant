using Vexora.Domain.sharedKernel.BaseEntities;
using Vexora.Domain.Validators.Users;

namespace Vexora.Domain.Entities.Users;

public class User : BaseEntity
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string Password { get; set; } = null!;

    #region Constractor

    private User()
    {

    }

    #endregion

    #region Functions

    public static User Register(string firstName, string lastName, string phoneNumber, string password)
    {
        UserValidations.FirstNameAndLastnameValidator(firstName, lastName);

        UserValidations.PhoneNumberValidation(phoneNumber);

        UserValidations.PasswordValidation(password);

        return new User
        {
            FirstName = firstName,
            LastName = lastName,
            PhoneNumber = phoneNumber,
            Password = password,
            CreateAt = DateTime.UtcNow,
            UpdateAt = DateTime.UtcNow,
        };
    }

    public void Update(string firstName, string lastName, string phoneNumber, string password)
    {
        UserValidations.FirstNameAndLastnameValidator(firstName, lastName);

        UserValidations.PhoneNumberValidation(phoneNumber);

        UserValidations.PasswordValidation(password);

        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
        Password = password;
        Update();

    }

    #endregion
}
