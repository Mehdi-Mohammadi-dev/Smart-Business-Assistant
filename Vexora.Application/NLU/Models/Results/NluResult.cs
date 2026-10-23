using Vexora.Application.NLU.Models.Entities;
using Vexora.Application.NLU.Models.Intents;

namespace Vexora.Application.NLU.Models.Results;

public sealed record NluResult
(
    string OriginalText,

    string NormalizedText,

    IntentType Intent,

    double IntentConfidence ,

    IReadOnlyCollection<IntentCandidate> Candidates,

    IReadOnlyCollection<EntityResult> Entities,

    DirectionHint Direction = DirectionHint.Unspecified
);
