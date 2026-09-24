using Vexora.Application.Conversations.Abstracts;
using Vexora.Application.Dtos.Customers;
using Vexora.Application.NLU.Abstractions.Extractions.Pipelines;
using Vexora.Application.NLU.Models.Entities;
using Vexora.Application.NLU.Models.Results;
using Vexora.Application.Services.Interfaces.Customers;

namespace Vexora.Application.Conversations.Implementations;

public sealed class ConversationProcessor(
    IEntityExtractionPipeline entityPipeline,
    ICustomerService customerService)
    : IConversationProcessor
{

    public async Task<ConversationProcessingResult> ProcessAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        var extractionResult =
            entityPipeline.Extract(text);


        var customer =
            extractionResult.Entities
            .FirstOrDefault(x =>
                x.Type == EntityType.Customer);


        if (customer is null)
        {
            return new ConversationProcessingResult(
                ProcessingStatus.Failed,
                "مشتری مشخص نیست",
                null);
        }


        var customerName =
            customer.RawValue;


        var resolution =
            await customerService.ResolveByNameAsync(
                customerName,
                cancellationToken);


        return resolution.Status switch
        {
            CustomerResolutionStatus.Resolved =>
                new ConversationProcessingResult(
                    ProcessingStatus.Success,
                    "مشتری پیدا شد",
                    resolution),


            CustomerResolutionStatus.Ambiguous =>
                new ConversationProcessingResult(
                    ProcessingStatus.NeedCustomerSelection,
                    "چند مشتری پیدا شد",
                    resolution),


            CustomerResolutionStatus.NotFound =>
                new ConversationProcessingResult(
                    ProcessingStatus.NeedCustomerCreation,
                    "مشتری وجود ندارد",
                    resolution),


            _ =>
                new ConversationProcessingResult(
                    ProcessingStatus.Failed,
                    "خطای پردازش",
                    null)
        };
    }
}
