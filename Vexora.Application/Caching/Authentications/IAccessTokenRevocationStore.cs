namespace Vexora.Application.Caching.Authentications;

public interface IAccessTokenRevocationStore
{
    Task RevokeAsync(
        string tokenId,
        DateTimeOffset expiresAt,
        CancellationToken cancellationToken = default);

    Task<bool> IsRevokedAsync(
        string tokenId,
        CancellationToken cancellationToken = default);
}
