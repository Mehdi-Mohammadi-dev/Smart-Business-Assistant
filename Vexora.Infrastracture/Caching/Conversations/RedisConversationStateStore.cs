using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Vexora.Application.Conversations;
using Vexora.Application.Conversations.Interfaces;

namespace Vexora.Infrastracture.Caching.Conversations;

public sealed class RedisConversationStateStore(
    IDistributedCache cache)
    : IConversationStateStore
{
    private const string KeyPrefix = "vexora:conversation:";

    public async Task<ConversationState?> GetAsync(
        string conversationId,
        CancellationToken cancellationToken = default)
    {
        var key = BuildKey(conversationId);

        var value = await cache.GetAsync(
            key,
            cancellationToken);

        if (value is null)
            return null;

        var json = Encoding.UTF8.GetString(value);

        return JsonSerializer.Deserialize<ConversationState>(json);
    }   

    public async Task SaveAsync(
        ConversationState state,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(state);

        var key = BuildKey(state.ConversationId);

        var json = JsonSerializer.Serialize(state);

        var value = Encoding.UTF8.GetBytes(json);

        await cache.SetAsync(
            key,
            value,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow =
                    TimeSpan.FromMinutes(30)
            },
            cancellationToken);
    }

    public async Task RemoveAsync(
        string conversationId,
        CancellationToken cancellationToken = default)
    {
        var key = BuildKey(conversationId);

        await cache.RemoveAsync(
            key,
            cancellationToken);
    }

    private static string BuildKey(string conversationId)
    {
        return $"{KeyPrefix}{conversationId}";
    }
}