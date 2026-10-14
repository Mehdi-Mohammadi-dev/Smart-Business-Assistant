namespace Vexora.Application.Dtos.Authentications;

public record SendOtpDto
    (
        string FirstName,
    string LastName,
    string PhoneNumber,
    string Password
);
