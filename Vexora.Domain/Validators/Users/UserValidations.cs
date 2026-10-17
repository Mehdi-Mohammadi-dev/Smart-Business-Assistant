using System;
using System.Collections.Generic;
using System.Text;
using Vexora.Domain.Exceptions.Customers;
using Vexora.Domain.Exceptions.Users;

namespace Vexora.Domain.Validators.Users;

public static class UserValidations
{
    public static void ValidatName(string firstName, string lastname)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new UserExceptions.FirstNameInvalidException();

        if (string.IsNullOrWhiteSpace(lastname))
            throw new UserExceptions.LastNameInvalidException();
    }

    public static void PhoneNumberValidation(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber) ||
            !System.Text.RegularExpressions.Regex.IsMatch(phoneNumber, @"^09\d{9}$"))
            throw new UserExceptions.PhoneNumberInvalidException();
    }

    public static void PasswordValidation(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new UserExceptions.NotStrongPasswordException();
    }
}
