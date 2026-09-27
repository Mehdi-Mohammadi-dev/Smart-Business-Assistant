namespace Vexora.Application.Conversations.Interfaces;  

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
