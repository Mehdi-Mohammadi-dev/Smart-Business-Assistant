using Vexora.Application.Platform.Statements;
using Vexora.Infrastracture.Pdf;

namespace Vexora.Api.Platform;

public static class PlatformServiceCollectionExtensions
{
    public static IServiceCollection AddPlatformServices(this IServiceCollection services)
    {
        services.AddScoped<IStatementPdfGenerator, QuestPdfStatementGenerator>();
        return services;
    }
}
