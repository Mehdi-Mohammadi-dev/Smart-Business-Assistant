using Vexora.Domain.Entities.Users;
using Vexora.Domain.sharedKernel.BaseEntities;
using Vexora.Domain.sharedKernel.Validators;
using Vexora.Domain.Validators.RefreshTokens;

namespace Vexora.Domain.Entities.RefreshTokens;

public class RefreshToken : UserBaseEntity
{
    public string TokenHash { get; private set; } = null!;

    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }

    public string? ReplacedByTokenHash { get; private set; }

    #region Relations

    public User User { get; private set; } = null!;

    #endregion

    #region Constractors

    private RefreshToken()
    {
    }

    #endregion

    #region Functions

    public static RefreshToken Create(
        long userId,
        string tokenHash,
        DateTimeOffset expiresAt)
    {
        Guard.AgainstInvalidId(userId);
        RefreshTokenValidations.AgainstTokenHash(tokenHash);
        RefreshTokenValidations.AgainstExpiresDate(expiresAt);

        return new RefreshToken
        {
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAt = expiresAt
        };
    }

    public void Revoke(string? replacedByTokenHash = null)
    {
        if (RevokedAt.HasValue)
            return;

        RevokedAt = DateTimeOffset.UtcNow;
        ReplacedByTokenHash = replacedByTokenHash;
    }

    #endregion
}
