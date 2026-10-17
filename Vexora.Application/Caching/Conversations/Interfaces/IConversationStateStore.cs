using Vexora.Application.Caching.Conversations;

namespace Vexora.Application.Caching.Conversations.Interfaces;

public interface IConversationStateStore
{
    Task<ConversationState?> GetAsync(
        string conversationId,
        CancellationToken cancellationToken = default);

    Task SaveAsync(
        ConversationState state,
        CancellationToken cancellationToken = default);

    Task RemoveAsync(
        string conversationId,
        CancellationToken cancellationToken = default);
}
