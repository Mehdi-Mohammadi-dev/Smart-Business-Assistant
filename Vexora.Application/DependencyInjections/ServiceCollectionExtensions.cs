using Microsoft.Extensions.DependencyInjection;
using Vexora.Application.Conversations.Implementations;
using Vexora.Application.Conversations.Interfaces;
using Vexora.Application.NLU.Abstractions.Dates;
using Vexora.Application.NLU.Abstractions.Extractions;
using Vexora.Application.NLU.Abstractions.Extractions.Customers;
using Vexora.Application.NLU.Abstractions.Extractions.Money;
using Vexora.Application.NLU.Abstractions.Extractions.Pipelines;
using Vexora.Application.NLU.Abstractions.Money;
using Vexora.Application.NLU.Abstractions.Number;
using Vexora.Application.NLU.Abstractions.Pipeline;
using Vexora.Application.NLU.Extractions;
using Vexora.Application.NLU.Extractions.Dates;
using Vexora.Application.NLU.Extractions.Pipelines;
using Vexora.Application.NLU.Normalizations.Parsing.Money;
using Vexora.Application.Services.Implementations.Chats;
using Vexora.Application.Services.Implementations.Checks;
using Vexora.Application.Services.Implementations.Customers;
using Vexora.Application.Services.Implementations.Payments;
using Vexora.Application.Services.Interfaces.Chats;
using Vexora.Application.Services.Interfaces.Checks;
using Vexora.Application.Services.Interfaces.Customers;
using Vexora.Application.Services.Interfaces.Payments;
using Vexora.Application.NLU.Abstractions;
using Vexora.Application.NLU.Abstractions.Intents;
using Vexora.Application.NLU.Extractions.Customers;
using Vexora.Application.NLU.Intent;
using Vexora.Application.NLU.Normalizations;
using Vexora.Application.NLU.Parsing.Number;
using Vexora.Application.NLU.Pipeline;

namespace Vexora.Application.DependencyInjections;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddSingleton<ITextNormalizer, PersianTextNormalizer>();

        services.AddSingleton<IPersianNumberParser, PersianNumberParser>();

        services.AddSingleton<IMoneyParser, MoneyParser>();

        services.AddSingleton<ICustomerExtractor, CustomerExtractor>();

        services.AddSingleton<IDateExtractor, PersianDateExtractor>();

        services.AddScoped<ICheckService, CheckService>();

        services.AddSingleton<IIntentDetector, IntentDetector>();

        services.AddSingleton<IMoneyExtractor, MoneyExtractor>();

        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<IConversationProcessor,ConversationProcessor>();

        services.AddSingleton<IEntityExtractor>(
            sp => sp.GetRequiredService<IMoneyExtractor>());

        services.AddSingleton<IEntityExtractionPipeline, EntityExtractionPipeline>();

        services.AddSingleton<INluPipeline, NluPipeline>();

        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IPaymentService, PaymentService>();

        return services;
    }
}