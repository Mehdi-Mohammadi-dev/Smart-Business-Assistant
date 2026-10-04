using Vexora.Application.NLU.Abstractions.Extractions.Pipelines;
using Vexora.Application.NLU.Abstractions.Pipeline;
using Vexora.Application.NLU.Models.Intents;
using Vexora.Application.NLU.Models.Results;
using Vexora.Application.NLU.Abstractions;
using Vexora.Application.NLU.Abstractions.Intents;

namespace Vexora.Application.NLU.Pipeline;

public sealed class NluPipeline : INluPipeline
{
    private readonly ITextNormalizer _normalizer;
    private readonly IIntentDetector _intentDetector;
    private readonly IEntityExtractionPipeline _entityExtractionPipeline;

    public NluPipeline(
        ITextNormalizer normalizer,
        IIntentDetector intentDetector,
        IEntityExtractionPipeline entityExtractionPipeline)
    {
        _normalizer = normalizer;
        _intentDetector = intentDetector;
        _entityExtractionPipeline = entityExtractionPipeline;
    }

    public NluResult Analyze(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        var normalized = _normalizer.Normalize(text);

        if (string.IsNullOrWhiteSpace(normalized.Value))
        {
            return new NluResult
            (
                normalized.Original,
                string.Empty,
                IntentType.Unknown,
                0,
                [], []
            );
        }

        var intent = _intentDetector.Detect(normalized.Value);

        var extraction =
            _entityExtractionPipeline.Extract(normalized.Value);

        return new NluResult
        (
            normalized.Original,
            normalized.Value,
            intent.Intent,
            intent.Confidence,
            intent.Candidates,
            extraction.Entities
        );
    }
}