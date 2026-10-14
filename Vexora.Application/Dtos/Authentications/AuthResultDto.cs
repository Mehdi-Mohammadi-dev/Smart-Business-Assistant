using Vexora.Application.Dtos.Authentications.Profiles;

namespace Vexora.Application.Dtos.Authentications;

public record AuthResultDto(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt,
    ProfileDto Profile);
