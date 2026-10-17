namespace Vexora.Application.NLU.Models.Entities;

public sealed record EntityResult
(
    EntityType Type,

    string RawValue,

    object? Value ,

    int Start,

    int Length,

    double Confidence
);
