namespace Vexora.Api.Contracts.Authentications;

public record RegisterContract
(
    string RegistrationToken,
    string OtpCode,
    string? ReferralCode = null
);
