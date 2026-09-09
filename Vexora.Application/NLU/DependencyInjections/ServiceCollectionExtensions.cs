using Microsoft.Extensions.DependencyInjection;
using Vexora.Application.NLU.Abstractions.Money;
using Vexora.Application.NLU.Abstractions.Number;
using Voxora.Application.NLU.Abstractions;
using Voxora.Application.NLU.Entities.Money;
using Voxora.Application.NLU.Entities.Number;
using Voxora.Application.NLU.Normalizations;

namespace Voxora.Application.DependencyInjections;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddSingleton<ITextNormalizer, PersianTextNormalizer>();

        services.AddSingleton<IPersianNumberParser, PersianNumberParser>();

        services.AddSingleton<IMoneyParser, MoneyParser>();

        return services;
    }
}