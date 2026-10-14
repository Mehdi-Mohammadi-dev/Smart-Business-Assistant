namespace Vexora.Application.Dtos.Authentications;

public record SendOtpResultDto
(
    string RegistrationToken,
    DateTimeOffset ExpiresAt);
