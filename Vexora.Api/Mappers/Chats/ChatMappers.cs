using Vexora.Api.Contracts.Chats;
using Vexora.Application.Dtos.Chats;

namespace Vexora.Api.Mappers.Chats;

public static class ChatMappers
{
    public static ChatRequestDto ChatRequestMapToDto(
        this ChatRequest request,
        string conversationId)
    =>
        new ChatRequestDto
        (
            conversationId,
            request.Message
        );

    public static ChatResponse ChatResponseDtoMapToContract(this ChatResponseDto response)
        =>
          new ChatResponse
        (
              response.Message,
              response.RequiresUserInput,
              response.Options.ChatOptionDtoMapToResponseOption()
        );

    private static IReadOnlyCollection<ChatResponseOption> ChatOptionDtoMapToResponseOption(this IReadOnlyCollection<ChatOptionDto> dtos)
        =>
        dtos.Select(dto => new ChatResponseOption
    (
        dto.Number,
        dto.Label,
        dto.Type,
        dto.CustomerId
    )).ToList().AsReadOnly();
}
