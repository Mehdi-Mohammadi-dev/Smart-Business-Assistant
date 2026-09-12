using Vexora.Domain.Constants.Users;

namespace Vexora.Domain.Exceptions.Users;

public static class UserExceptions
{
    public sealed class PhoneNumberInvalidException() : Exception
        (UserConstants.PhoneNumberInvalid);

    public sealed class FirstNameInvalidException() : Exception
    (UserConstants.FirstNameInvalid);

    public sealed class LastNameInvalidException() : Exception
    (UserConstants.LastNameInvalid);

    public sealed class NotStrongPasswordException() : Exception
    (UserConstants.NotStrongPassword);
}
