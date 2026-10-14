namespace Vexora.Api.Contracts.Authentications.Users;

public record UserContract
(
    long Id,
    string FirstName,
    string LastName,
    string PhoneNumber);
