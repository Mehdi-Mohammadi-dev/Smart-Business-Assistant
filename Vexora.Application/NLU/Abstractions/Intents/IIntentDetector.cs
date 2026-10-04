using Vexora.Application.NLU.Models.Intents;

namespace Vexora.Application.NLU.Abstractions.Intents;

public interface IIntentDetector
{
    IntentDetectionResult Detect(string normalizedText);
}