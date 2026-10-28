namespace Vexora.Application.Dtos.Authentications;

public record RegisterDto
(
    string RegistrationToken,
    string OtpCode,
    string? ReferralCode = null
);
