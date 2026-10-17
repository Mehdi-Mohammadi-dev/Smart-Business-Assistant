namespace Vexora.Api.Contracts.Chats;

public record ChatResponse
(
    string Message,
    bool RequiresUserInput,
    IReadOnlyCollection<ChatResponseOption> Options
);
