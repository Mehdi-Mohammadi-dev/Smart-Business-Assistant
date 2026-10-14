using Vexora.Domain.Entities.Users;

namespace Vexora.Application.Services.Interfaces.Authentications;

public record AccessTokenResult(string Token, DateTimeOffset ExpiresAt);

public interface ITokenService
{
    TimeSpan RefreshTokenLifetime { get; }

    AccessTokenResult CreateAccessToken(User user);

    // A random value that is given to the client.
    string GenerateRefreshToken();

    // Only the hash is stored in the database.
    string HashRefreshToken(string refreshToken);
}
