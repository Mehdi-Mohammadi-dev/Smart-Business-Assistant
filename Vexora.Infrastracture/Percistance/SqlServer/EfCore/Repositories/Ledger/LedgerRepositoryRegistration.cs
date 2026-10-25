using Microsoft.Extensions.DependencyInjection;
using Vexora.Application.Ledger.Repositories;
using Vexora.Infrastracture.Seeding;

namespace Vexora.Infrastracture.Percistance.SqlServer.EfCore.Repositories.Ledger;

public static class LedgerRepositoryRegistration
{
    public static IServiceCollection AddLedgerRepositories(this IServiceCollection services)
    {
        services.AddScoped<ILedgerCustomerReader, LedgerCustomerReader>();
        services.AddScoped<ILedgerPaymentReader, LedgerPaymentReader>();
        services.AddScoped<ILedgerCheckReader, LedgerCheckReader>();
        services.AddScoped<ILedgerSummaryReader, LedgerSummaryReader>();
        services.AddScoped<IStatementReader, StatementReader>();

        services.AddHostedService<DevelopmentDataSeeder>();

        return services;
    }
}
