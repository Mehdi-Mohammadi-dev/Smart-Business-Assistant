using Microsoft.Extensions.DependencyInjection;
using Vexora.Application.NLU.Abstractions.Extractions;
using Vexora.Application.NLU.Abstractions.Extractions.Money;
using Vexora.Application.NLU.Abstractions.Extractions.Pipelines;
using Vexora.Application.NLU.Abstractions.Money;
using Vexora.Application.NLU.Abstractions.Number;
using Vexora.Application.NLU.Abstractions.Pipeline;
using Vexora.Application.NLU.Extractions;
using Vexora.Application.NLU.Extractions.Pipelines;
using Vexora.Application.NLU.Normalizations.Parsing.Money;
using Vexora.Application.Services.Implementations.Customers;
using Vexora.Application.Services.Interfaces.Customers;
using Voxora.Application.NLU.Abstractions;
using Voxora.Application.NLU.Abstractions.Intents;
using Voxora.Application.NLU.Intent;
using Voxora.Application.NLU.Normalizations;
using Voxora.Application.NLU.Parsing.Number;
using Voxora.Application.NLU.Pipeline;

namespace Vexora.Application.DependencyInjections;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddSingleton<ITextNormalizer, PersianTextNormalizer>();

        services.AddSingleton<IPersianNumberParser, PersianNumberParser>();

        services.AddSingleton<IMoneyParser, MoneyParser>();

        services.AddSingleton<IIntentDetector, IntentDetector>();

        services.AddSingleton<IMoneyExtractor, MoneyExtractor>();

        services.AddSingleton<IEntityExtractor>(
            sp => sp.GetRequiredService<IMoneyExtractor>());

        services.AddSingleton<IEntityExtractionPipeline, EntityExtractionPipeline>();

        services.AddSingleton<INluPipeline, NluPipeline>();

        services.AddScoped<ICustomerService, CustomerService>();

        return services;
    }
}