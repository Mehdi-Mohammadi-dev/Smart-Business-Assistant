using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using Vexora.Application.Conversations;
using Vexora.Application.Conversations.Abstracts;

namespace Vexora.Infrastracture.Caching.Conversations;

public sealed class RedisConversationStateStore
    : IConversationStateStore
{
    private readonly IDistributedCache _cache;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public RedisConversationStateStore(
        IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<ConversationState?> GetAsync(
        string conversationId,
        CancellationToken cancellationToken = default)
    {
        var key = GetKey(conversationId);

        var json = await _cache.GetStringAsync(
            key,
            cancellationToken);

        if (string.IsNullOrWhiteSpace(json))
            return null;

        return JsonSerializer.Deserialize<ConversationState>(
            json,
            JsonOptions);
    }

    public async Task SaveAsync(
        ConversationState state,
        CancellationToken cancellationToken = default)
    {
        var key = GetKey(state.ConversationId);

        var json = JsonSerializer.Serialize(
            state,
            JsonOptions);

        await _cache.SetStringAsync(
            key,
            json,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow =
                    TimeSpan.FromMinutes(30)
            },
            cancellationToken);
    }

    public Task RemoveAsync(
        string conversationId,
        CancellationToken cancellationToken = default)
    {
        return _cache.RemoveAsync(
            GetKey(conversationId),
            cancellationToken);
    }

    private static string GetKey(
        string conversationId)
    {
        return $"conversation:{conversationId}";
    }
}
