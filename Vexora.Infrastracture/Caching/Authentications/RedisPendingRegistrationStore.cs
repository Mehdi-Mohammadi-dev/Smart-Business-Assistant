using Microsoft.Extensions.Caching.Distributed;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Vexora.Application.Dtos.Authentications;
using Vexora.Application.Services.Interfaces.Authentications;

namespace Vexora.Infrastracture.Caching.Authentications;

public sealed class RedisPendingRegistrationStore(
    IDistributedCache cache) : IPendingRegistrationStore
{
    private const string KeyPrefix = "vexora:pending-registration:";

    private static string GetKey(string token)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return KeyPrefix + Convert.ToHexString(hash);
    }

    public async Task StoreAsync(
        string token,
        PendingRegistration registration,
        TimeSpan lifetime,
        CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(registration);

        await cache.SetStringAsync(
            GetKey(token),
            json,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = lifetime
            },
            cancellationToken);
    }

    public async Task<PendingRegistration?> GetAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        var json = await cache.GetStringAsync(
            GetKey(token), cancellationToken);

        return json is null
            ? null
            : JsonSerializer.Deserialize<PendingRegistration>(json);
    }

    public Task RemoveAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        return cache.RemoveAsync(GetKey(token), cancellationToken);
    }
}
