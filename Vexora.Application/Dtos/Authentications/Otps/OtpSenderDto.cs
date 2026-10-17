namespace Vexora.Application.Dtos.Authentications.Otps;

public record OtpSenderDto
(
    string PhoneNumber,
    string Code
);
