using Vexora.Application.NLU.Models.Intents;

namespace Voxora.Application.NLU.Abstractions.Intents;

public interface IIntentDetector
{
    IntentDetectionResult Detect(string normalizedText);
}