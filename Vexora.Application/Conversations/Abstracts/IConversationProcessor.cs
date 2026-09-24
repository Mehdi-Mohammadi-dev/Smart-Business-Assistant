using Vexora.Application.NLU.Models.Results;

namespace Vexora.Application.Conversations.Abstracts;

public interface IConversationProcessor
{
    Task<ConversationProcessingResult> ProcessAsync(
        string text,
        CancellationToken cancellationToken = default);
}
