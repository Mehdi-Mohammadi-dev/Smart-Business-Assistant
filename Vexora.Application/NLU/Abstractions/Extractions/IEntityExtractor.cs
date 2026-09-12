using Vexora.Application.NLU.Models.Entities;

namespace Vexora.Application.NLU.Abstractions.Extractions;

public interface IEntityExtractor
{
    EntityType EntityType { get; }

    IReadOnlyCollection<EntityResult> Extract(string normalizedText);
}


