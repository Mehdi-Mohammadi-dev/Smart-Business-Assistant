namespace Vexora.Application.NLU.Models.Results;

public sealed record ConversationProcessingResult
(
    ProcessingStatus Status,
    string? Message,
    object? Data
);
