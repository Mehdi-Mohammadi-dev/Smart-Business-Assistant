namespace Vexora.Api.Contracts.Authentications.Profiles;

public sealed record UpdateProfileContract(string FirstName, string LastName, string? BusinessName = null);
