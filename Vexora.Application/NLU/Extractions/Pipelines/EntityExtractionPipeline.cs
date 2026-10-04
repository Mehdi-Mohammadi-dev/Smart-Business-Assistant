using Vexora.Application.NLU.Abstractions.Dates;
using Vexora.Application.NLU.Abstractions.Extractions;
using Vexora.Application.NLU.Abstractions.Extractions.Customers;
using Vexora.Application.NLU.Abstractions.Extractions.Money;
using Vexora.Application.NLU.Abstractions.Extractions.Pipelines;
using Vexora.Application.NLU.Models.Entities;

namespace Vexora.Application.NLU.Extractions.Pipelines;

public sealed class EntityExtractionPipeline(
 IMoneyExtractor moneyExtractor,
 ICustomerExtractor customerExtractor,
 IDateExtractor dateExtractor)
        : IEntityExtractionPipeline
{
    public EntityExtractionResult Extract(string normalizedText)
    {
        var entities = new List<EntityResult>();

        foreach (var money in moneyExtractor.Extract(normalizedText))
        {
            entities.Add(new EntityResult(
                EntityType.Money,
                money.RawValue,
                money.Value,
                money.Start,
                money.Length,
                money.Confidence));
        }

        foreach (var customer in customerExtractor.Extract(normalizedText))
        {
            entities.Add(customer);
        }

        var date = dateExtractor.Extract(normalizedText);

        if (date.HasValue)
        {
            entities.Add(
                new EntityResult(
                    EntityType.Date,
                    date.Value.ToString("yyyy-MM-dd"),
                    date.Value,
                    0,
                    0,
                    0.90));
        }

        return new EntityExtractionResult(entities);
    }
}