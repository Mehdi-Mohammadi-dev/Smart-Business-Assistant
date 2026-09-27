namespace Vexora.Application.Dtos.Chats;

public sealed record ChatResponseDto
(
    string Message,
    bool RequiresUserInput,
    IReadOnlyCollection<ChatOptionDto> Options
);
