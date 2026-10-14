namespace Vexora.Api.Contracts.Authentications;

public record RegisterContract
(
    string RegistrationToken,
    string OtpCode
);
