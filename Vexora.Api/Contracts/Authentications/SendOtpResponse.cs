namespace Vexora.Api.Contracts.Authentications;

public record SendOtpResponse(
    string RegistrationToken,
    DateTimeOffset ExpiresAt);
