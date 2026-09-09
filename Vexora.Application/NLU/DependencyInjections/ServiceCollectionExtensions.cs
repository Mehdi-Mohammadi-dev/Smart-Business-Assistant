using Microsoft.Extensions.DependencyInjection;
using Vexora.Application.NLU.Abstractions.Money;
using Vexora.Application.NLU.Abstractions.Number;
using Vexora.Application.NLU.Normalizations.Parsing.Money;
using Voxora.Application.NLU.Abstractions;
using Voxora.Application.NLU.Abstractions.Intents;
using Voxora.Application.NLU.Intent;
using Voxora.Application.NLU.Normalizations;
using Voxora.Application.NLU.Parsing.Number;

namespace Voxora.Application.DependencyInjections;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddSingleton<ITextNormalizer, PersianTextNormalizer>();

        services.AddSingleton<IPersianNumberParser, PersianNumberParser>();

        services.AddSingleton<IMoneyParser, MoneyParser>();

        services.AddSingleton<IIntentDetector, IntentDetector>();

        return services;
    }
}