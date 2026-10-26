namespace Vexora.Api.Contracts.Authentications;

public record SendOtpContract
(
    string FirstName,
    string LastName,
    string PhoneNumber,
    string Password,
    string? BusinessName = null
);
