using FluentAssertions;
using Vexora.Application.Caching.Conversations;
using Vexora.Application.Caching.Conversations.Implementations;
using Vexora.Application.Caching.Conversations.Interfaces;
using Vexora.Application.Constants.Messages.Conversations;
using Vexora.Application.Dtos.Chats;
using Vexora.Application.Dtos.Checks;
using Vexora.Application.Dtos.Customers;
using Vexora.Application.Dtos.Payments;
using Vexora.Application.NLU.Extractions;
using Vexora.Application.NLU.Extractions.Customers;
using Vexora.Application.NLU.Extractions.Dates;
using Vexora.Application.NLU.Extractions.Pipelines;
using Vexora.Application.NLU.Intent;
using Vexora.Application.NLU.Models.Intents;
using Vexora.Application.NLU.Models.Results;
using Vexora.Application.NLU.Normalizations;
using Vexora.Application.NLU.Normalizations.Parsing.Money;
using Vexora.Application.NLU.Parsing.Number;
using Vexora.Application.NLU.Pipeline;
using Vexora.Application.Services.Interfaces.Checks;
using Vexora.Application.Services.Interfaces.Customers;
using Vexora.Application.Services.Interfaces.Payments;
using Xunit;

namespace Vexora.Application.Test.NLU.ConversationTests.ConversationOverrideTestsHelpers;

internal sealed class InMemoryStateStore : IConversationStateStore
{
    private readonly Dictionary<string, ConversationState> _states = new();

    public Task<ConversationState?> GetAsync(string conversationId, CancellationToken cancellationToken = default)
        => Task.FromResult(_states.GetValueOrDefault(conversationId));

    public Task SaveAsync(ConversationState state, CancellationToken cancellationToken = default)
    {
        _states[state.ConversationId] = state;
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string conversationId, CancellationToken cancellationToken = default)
    {
        _states.Remove(conversationId);
        return Task.CompletedTask;
    }
}
