namespace Vexora.Application.Dtos.Authentications;

public record PendingRegistration
(
    long OtpId,
    string FirstName,
    string LastName,
    string PhoneNumber,
    string PasswordHash);
