using Vexora.Application.Dtos.Chats;
using Vexora.Application.NLU.Models.Results;

namespace Vexora.Application.Caching.Conversations.Interfaces;  

public interface IConversationProcessor
{
    Task<ConversationProcessingResult> ProcessAsync(
           ChatRequestDto request,
           CancellationToken cancellationToken = default);
}
