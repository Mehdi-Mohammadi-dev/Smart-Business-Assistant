using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Vexora.Api.Platform;

public sealed class RedisHealthCheck(IDistributedCache cache) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            const string key = "vexora:health:ready";
            await cache.SetStringAsync(key, "ok", new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5) }, cancellationToken);
            return await cache.GetStringAsync(key, cancellationToken) == "ok" ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy();
        }
        catch { return HealthCheckResult.Unhealthy(); }
    }
}
