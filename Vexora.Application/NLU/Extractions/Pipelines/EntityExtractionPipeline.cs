using Vexora.Application.NLU.Abstractions.Extractions;
using Vexora.Application.NLU.Abstractions.Extractions.Pipelines;
using Vexora.Application.NLU.Models.Entities;

namespace Vexora.Application.NLU.Extractions.Pipelines;

public sealed class EntityExtractionPipeline
    : IEntityExtractionPipeline
{
    private readonly IEnumerable<IEntityExtractor> _extractors;

    public EntityExtractionPipeline(
        IEnumerable<IEntityExtractor> extractors)
    {
        _extractors = extractors;
    }

    public EntityExtractionResult Extract(string normalizedText)
    {
        if (string.IsNullOrWhiteSpace(normalizedText))
        {
            return new EntityExtractionResult([]);
        }

        var entities = new List<EntityResult>();

        foreach (var extractor in _extractors)
        {
            var extracted = extractor.Extract(normalizedText);

            entities.AddRange(extracted);
        }

        return new EntityExtractionResult
        (
            entities.OrderBy(x => x.Start)
                .ToArray()
        );
    }
}
