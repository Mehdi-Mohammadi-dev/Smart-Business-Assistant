using Microsoft.EntityFrameworkCore;
using Vexora.Application.Common;
using Vexora.Application.Ledger.Models;
using Vexora.Application.Ledger.Queries;
using Vexora.Application.Ledger.Repositories;
using Vexora.Infrastracture.Percistance.SqlServer.EfCore.Context;

namespace Vexora.Infrastracture.Percistance.SqlServer.EfCore.Repositories.Ledger;

public sealed class StatementReader(ApplicationDbContext context) : IStatementReader
{
    public async Task<string?> GetCustomerNameAsync(
        long userId,
        long customerId,
        CancellationToken cancellationToken = default)
    {
        var customer = await context.Customers
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.Id == customerId)
            .Select(x => new { x.FirstName, x.LastName })
            .FirstOrDefaultAsync(cancellationToken);

        return customer is null
            ? null
            : PersonName.Full(customer.FirstName, customer.LastName);
    }

    public async Task<int> CountRowsAsync(
        long userId,
        long customerId,
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken = default)
    {
        var paymentsCount = await PaymentQueries
            .ForStatement(context.Payments.AsNoTracking(), userId, customerId, DateRangeBounds.FromIranDays(from, to))
            .CountAsync(cancellationToken);

        var checksCount = await CheckQueries
            .ForStatement(context.Checks.AsNoTracking(), userId, customerId, from, to)
            .CountAsync(cancellationToken);

        return paymentsCount + checksCount;
    }

    public async Task<IReadOnlyList<StatementPaymentRow>> GetPaymentsAsync(
        long userId,
        long customerId,
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken = default)
        => await PaymentQueries
            .ForStatement(context.Payments.AsNoTracking(), userId, customerId, DateRangeBounds.FromIranDays(from, to))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<StatementCheckRow>> GetChecksAsync(
        long userId,
        long customerId,
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken = default)
        => await CheckQueries
            .ForStatement(context.Checks.AsNoTracking(), userId, customerId, from, to)
            .ToListAsync(cancellationToken);
}
