namespace Vexora.Application.Dtos.Authentications.Otps;

public record SendOtpResultDto
(
    string RegistrationToken,
    DateTimeOffset ExpiresAt);
