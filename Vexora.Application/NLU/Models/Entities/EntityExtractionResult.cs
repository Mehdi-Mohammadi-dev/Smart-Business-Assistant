namespace Vexora.Application.NLU.Models.Entities;

public sealed record EntityExtractionResult
(
    IReadOnlyCollection<EntityResult> Entities
);
