namespace Vexora.Application.Dtos.Chats;

public sealed record ChatRequestDto
(
    string ConversationId,
    
    string Message
);
