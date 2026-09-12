using Vexora.Application.Dtos.Customers;

namespace Vexora.Application.NLU.Abstractions.Extractions.Customers;

public interface ICustomerExtractor
{
    IReadOnlyCollection<CustomerMention> Extract(
        string normalizedText);
}
