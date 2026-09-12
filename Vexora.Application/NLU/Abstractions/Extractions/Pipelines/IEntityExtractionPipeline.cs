using Vexora.Application.NLU.Models.Entities;

namespace Vexora.Application.NLU.Abstractions.Extractions.Pipelines;

public interface IEntityExtractionPipeline
{
    EntityExtractionResult Extract(string normalizedText);
}
