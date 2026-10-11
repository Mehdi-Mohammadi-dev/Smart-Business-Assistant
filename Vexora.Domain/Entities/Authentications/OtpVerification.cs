using Vexora.Domain.Entities.Users;
using Vexora.Domain.Enums.Authentications;
using Vexora.Domain.sharedKernel.BaseEntities;
using Vexora.Domain.Validators.Authentications;

namespace Vexora.Domain.Entities.Authentications;

public class OtpVerification : BaseEntity
{
    public long? UserId { get; private set; }

    public string PhoneNumber { get; private set; } = null!;
    public string CodeHash { get; private set; } = null!;

    public OtpPurpose Purpose { get; private set; }

    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? UsedAt { get; private set; }

    public int AttemptCount { get; private set; }

    #region Relations

    public User? User { get; private set; }

    #endregion

    #region Constractors

    private OtpVerification()
    {
    }

    #endregion

    #region Functions

    public static OtpVerification Create(
        string phoneNumber,
        string codeHash,
        OtpPurpose purpose,
        DateTimeOffset expiresAt,
        long? userId = null)
    {
        AuthenticationValidations.AgainstPhoneNumber(phoneNumber);

        if (string.IsNullOrWhiteSpace(codeHash))
            throw new ArgumentException(
                "Code hash is required.",
                nameof(codeHash));

        if (userId.HasValue && userId.Value <= 0)
            throw new ArgumentOutOfRangeException(nameof(userId));

        if (expiresAt <= DateTimeOffset.UtcNow)
            throw new ArgumentException(
                "Expiration must be in the future.",
                nameof(expiresAt));

        return new OtpVerification
        {
            PhoneNumber = phoneNumber,
            CodeHash = codeHash,
            Purpose = purpose,
            ExpiresAt = expiresAt,
            UserId = userId,
            AttemptCount = 0
        };
    }

    public void RegisterFailedAttempt()
    {
        if (UsedAt.HasValue || ExpiresAt <= DateTimeOffset.UtcNow)
            throw new InvalidOperationException(
                "OTP is no longer valid.");

        AttemptCount++;

        
    }

    public void MarkAsUsed()
    {
        if (UsedAt.HasValue || ExpiresAt <= DateTimeOffset.UtcNow)
            throw new InvalidOperationException(
                "OTP is no longer valid.");

        UsedAt = DateTimeOffset.UtcNow;

        
    }

    #endregion
}
