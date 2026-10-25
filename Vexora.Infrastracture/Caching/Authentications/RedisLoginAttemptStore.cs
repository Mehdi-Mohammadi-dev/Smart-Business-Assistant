using System.Globalization;
using Microsoft.Extensions.Caching.Distributed;
using Vexora.Application.Caching.Authentications;

namespace Vexora.Infrastracture.Caching.Authentications;

public sealed class RedisLoginAttemptStore(IDistributedCache cache) : ILoginAttemptStore
{
    private static string Key(string phone) => $"vexora:login-fail:{phone}";
    public async Task<bool> IsLockedAsync(string phoneNumber, CancellationToken cancellationToken = default)
        => await cache.GetStringAsync(Key(phoneNumber), cancellationToken) == "locked";

    public async Task RegisterFailureAsync(string phoneNumber, CancellationToken cancellationToken = default)
    {
        var key = Key(phoneNumber);
        var value = await cache.GetStringAsync(key, cancellationToken);
        var count = int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ? parsed : 0;
        count++;
        await cache.SetStringAsync(key, count >= 5 ? "locked" : count.ToString(CultureInfo.InvariantCulture),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) }, cancellationToken);
    }
    public Task ClearAsync(string phoneNumber, CancellationToken cancellationToken = default)
        => cache.RemoveAsync(Key(phoneNumber), cancellationToken);
}
