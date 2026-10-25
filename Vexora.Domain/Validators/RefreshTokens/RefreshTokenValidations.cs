using Vexora.Domain.Exceptions.RefreshTokens;

namespace Vexora.Domain.Validators.RefreshTokens;

public static class RefreshTokenValidations
{
    public static void AgainstTokenHash(string tokenHash)
    {
        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new RefreshTokenRequired();
    }

    public static void AgainstExpiresDate(DateTimeOffset expiresAt)
    {
        if (expiresAt <= DateTimeOffset.UtcNow)
            throw new InvalidExpiresDate();
    }
}
