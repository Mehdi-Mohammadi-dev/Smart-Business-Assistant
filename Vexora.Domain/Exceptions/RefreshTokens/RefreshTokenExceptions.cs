using Vexora.Domain.Constants.RefreshTokens;

namespace Vexora.Domain.Exceptions.RefreshTokens;

public static class RefreshTokenExceptions
{
    public sealed class RefreshTokenRequired() : Exception
        (RefreshTokenConstants.TokenHashRequired);

    public sealed class InvalidExpiresDate() : Exception
        (RefreshTokenConstants.ExpiresDate);
}
