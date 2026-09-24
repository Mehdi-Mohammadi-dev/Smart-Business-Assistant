using Vexora.Application.NLU.Abstractions.Extractions;
using Vexora.Application.NLU.Abstractions.Extractions.Customers;
using Vexora.Application.NLU.Abstractions.Extractions.Money;
using Vexora.Application.NLU.Abstractions.Extractions.Pipelines;
using Vexora.Application.NLU.Models.Entities;

namespace Vexora.Application.NLU.Extractions.Pipelines;

public sealed class EntityExtractionPipeline
    : IEntityExtractionPipeline
{
    private readonly IMoneyExtractor _moneyExtractor;
    private readonly ICustomerExtractor _customerExtractor;

    public EntityExtractionPipeline(
        IMoneyExtractor moneyExtractor,
        ICustomerExtractor customerExtractor)
    {
        _moneyExtractor = moneyExtractor;
        _customerExtractor = customerExtractor;
    }

    public EntityExtractionResult Extract(string normalizedText)
    {
        var entities = new List<EntityResult>();

        foreach (var money in _moneyExtractor.Extract(normalizedText))
        {
            entities.Add(new EntityResult(
                EntityType.Money,
                money.RawValue,
                money.Value,
                money.Start,
                money.Length,
                money.Confidence));
        }

        foreach (var customer in _customerExtractor.Extract(normalizedText))
        {
            entities.Add(new EntityResult(
                EntityType.Customer,
                customer.RawValue,
                customer,
                customer.Start,
                customer.Length,
                customer.Confidence));
        }

        return new EntityExtractionResult(entities);
    }
}