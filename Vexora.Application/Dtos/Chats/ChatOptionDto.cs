namespace Vexora.Application.Dtos.Chats;

public sealed record ChatOptionDto
(
    int Number,
    string Label,
    ChatOptionType Type,
    long? CustomerId
);
