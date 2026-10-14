using Vexora.Domain.Entities.RefreshTokens;

namespace Vexora.Application.Repositories.RefreshTokens;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);

    Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken = default);
}
