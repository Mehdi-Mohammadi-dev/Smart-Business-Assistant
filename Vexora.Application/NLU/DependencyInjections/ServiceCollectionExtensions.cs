using Microsoft.Extensions.DependencyInjection;
using Voxora.Application.NLU.Abstractions;
using Voxora.Application.NLU.Normalizations;

namespace Voxora.Application.DependencyInjections;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddSingleton<ITextNormalizer, PersianTextNormalizer>();

        return services;
    }
}