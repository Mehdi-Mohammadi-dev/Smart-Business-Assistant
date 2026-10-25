using Microsoft.Extensions.DependencyInjection;
using Vexora.Application.Ledger.Checks;
using Vexora.Application.Ledger.Customers;
using Vexora.Application.Ledger.Payments;
using Vexora.Application.Ledger.Statements;
using Vexora.Application.Ledger.Summary;

namespace Vexora.Application.Ledger;

public static class LedgerServiceCollectionExtensions
{
    public static IServiceCollection AddLedgerServices(this IServiceCollection services)
    {
        services.AddScoped<ILedgerCustomerService, LedgerCustomerService>();
        services.AddScoped<ILedgerPaymentService, LedgerPaymentService>();
        services.AddScoped<ILedgerCheckService, LedgerCheckService>();
        services.AddScoped<ILedgerSummaryService, LedgerSummaryService>();
        services.AddScoped<IStatementService, StatementService>();

        return services;
    }
}
