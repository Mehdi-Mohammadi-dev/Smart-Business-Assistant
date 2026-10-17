using Vexora.Domain.Entities.Users;

namespace Vexora.Application.Services.Interfaces.Authentications;

public record AccessTokenResult(string Token, DateTimeOffset ExpiresAt);

public interface ITokenService
{
    TimeSpan RefreshTokenLifetime { get; }

    AccessTokenResult CreateAccessToken(User user);

    string GenerateRefreshToken();

    string HashRefreshToken(string refreshToken);
}
