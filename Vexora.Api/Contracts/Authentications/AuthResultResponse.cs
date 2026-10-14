using Vexora.Api.Contracts.Authentications.Profiles;

namespace Vexora.Api.Contracts.Authentications;

public record AuthResultResponse
(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt,
    ProfileResponse Profile
);
