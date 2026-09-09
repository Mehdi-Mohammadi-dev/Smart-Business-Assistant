namespace Vexora.Application.NLU.Models.Intents;

public sealed record IntentDetectionResult
(
IntentType Intent,
double Confidence,
IReadOnlyCollection<IntentCandidate> Candidates
);
