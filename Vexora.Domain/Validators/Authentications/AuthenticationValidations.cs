using static Vexora.Domain.Exceptions.Authentications.AuthenticationExceptions;

namespace Vexora.Domain.Validators.Authentications;

public static class AuthenticationValidations
{
    public static void AgainstPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new PhoneNumberRequired();
    }
}
