namespace Vexora.Application.Dtos.Authentications;

public record RegisterDto
(
    string FirstName,
    string LastName,
    string PhoneNumber,
    string Password,
    string OtpCode
    );
