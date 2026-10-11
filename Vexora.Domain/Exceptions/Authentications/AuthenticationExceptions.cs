using Vexora.Domain.Constants.Authentications;

namespace Vexora.Domain.Exceptions.Authentications;

public static class AuthenticationExceptions
{
    public sealed class PhoneNumberRequired() : Exception
        (AuthenticationConstants.PhoneNumberRequired);
}
