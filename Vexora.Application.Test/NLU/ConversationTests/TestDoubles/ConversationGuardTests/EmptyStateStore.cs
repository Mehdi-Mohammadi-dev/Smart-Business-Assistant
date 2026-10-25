using Vexora.Application.Caching.Conversations;
using Vexora.Application.Caching.Conversations.Interfaces;

namespace Vexora.Application.Test.NLU.ConversationTests.ConversationGuardTestsHelpers;

internal sealed class EmptyStateStore : IConversationStateStore
{
    public Task<ConversationState?> GetAsync(string conversationId, CancellationToken cancellationToken = default)
        => Task.FromResult<ConversationState?>(null);

    public Task SaveAsync(ConversationState state, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    public Task RemoveAsync(string conversationId, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();
}
