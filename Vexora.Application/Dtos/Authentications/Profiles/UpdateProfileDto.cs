namespace Vexora.Application.Dtos.Authentications.Profiles;

public sealed record UpdateProfileDto(string FirstName, string LastName, string? BusinessName = null);
