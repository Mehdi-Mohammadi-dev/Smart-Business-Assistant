using Vexora.Domain.Exceptions.Authentications;

namespace Vexora.Domain.Validators.Authentications;

public static class AuthenticationValidations
{
    public static void AgainstPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new PhoneNumberRequired();
    }
}
