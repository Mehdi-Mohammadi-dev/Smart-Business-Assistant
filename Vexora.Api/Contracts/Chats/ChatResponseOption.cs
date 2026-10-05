using Vexora.Application.Dtos.Chats;

namespace Vexora.Api.Contracts.Chats;

public record ChatResponseOption
(
    int Number,
    string Label,
    ChatOptionType Type,
    long? CustomerId
);
