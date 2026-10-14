namespace Vexora.Application.Dtos.Authentications.Otps;

public record SendOtpDto
    (
        string FirstName,
    string LastName,
    string PhoneNumber,
    string Password
);
