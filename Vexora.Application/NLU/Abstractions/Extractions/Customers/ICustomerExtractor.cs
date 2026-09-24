using Vexora.Application.NLU.Models.Entities;

namespace Vexora.Application.NLU.Abstractions.Extractions.Customers;

public interface ICustomerExtractor
{
    EntityType EntityType { get; }

    IReadOnlyCollection<EntityResult> Extract(
        string normalizedText);
}
