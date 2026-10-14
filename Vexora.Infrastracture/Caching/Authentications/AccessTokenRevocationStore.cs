using Microsoft.Extensions.Caching.Distributed;
using Vexora.Application.Caching.Authentications;

namespace Vexora.Infrastracture.Caching.Authentications;

public sealed class RedisAccessTokenRevocationStore(
    IDistributedCache cache) : IAccessTokenRevocationStore
{
    private static string GetKey(string tokenId)
        => $"auth:revoked-access:{tokenId}";

    public async Task RevokeAsync(
        string tokenId,
        DateTimeOffset expiresAt,
        CancellationToken cancellationToken = default)
    {
        var remainingLifetime = expiresAt - DateTimeOffset.UtcNow;

        if (remainingLifetime <= TimeSpan.Zero)
            return;

        await cache.SetStringAsync(
            GetKey(tokenId),
            "revoked",
            new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = expiresAt
            },
            cancellationToken);
    }

    public async Task<bool> IsRevokedAsync(
        string tokenId,
        CancellationToken cancellationToken = default)
    {
        var value = await cache.GetStringAsync(
            GetKey(tokenId),
            cancellationToken);

        return value is not null;
    }
}
