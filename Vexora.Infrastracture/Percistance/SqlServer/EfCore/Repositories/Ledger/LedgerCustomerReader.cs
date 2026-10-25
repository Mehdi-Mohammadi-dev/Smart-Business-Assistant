using Microsoft.EntityFrameworkCore;
using Vexora.Application.Common;
using Vexora.Application.Ledger.Models;
using Vexora.Application.Ledger.Queries;
using Vexora.Application.Ledger.Repositories;
using Vexora.Infrastracture.Percistance.SqlServer.EfCore.Context;

namespace Vexora.Infrastracture.Percistance.SqlServer.EfCore.Repositories.Ledger;

public sealed class LedgerCustomerReader(ApplicationDbContext context) : ILedgerCustomerReader
{
    public async Task<IReadOnlyList<CustomerRef>> FindCustomersAsync(
        long userId,
        string? normalizedSearch,
        long? customerId,
        CancellationToken cancellationToken = default)
    {
        var query = context.Customers
            .AsNoTracking()
            .Where(x => x.UserId == userId);

        if (customerId is { } id)
            query = query.Where(x => x.Id == id);

        if (!string.IsNullOrEmpty(normalizedSearch))
        {
            var term = normalizedSearch;

            query = query.Where(x =>
                (x.FirstName + " " + x.LastName)
                    .Replace("ي", "ی")
                    .Replace("ك", "ک")
                    .Replace("\u200c", " ")
                    .Contains(term));
        }

        var rows = await query
            .Select(x => new { x.Id, x.FirstName, x.LastName })
            .ToListAsync(cancellationToken);

        return rows
            .Select(x => new CustomerRef(x.Id, PersonName.Full(x.FirstName, x.LastName)))
            .ToList();
    }

    public async Task<IReadOnlyList<PaymentAggregate>> GetPaymentAggregatesAsync(
        long userId,
        long? customerId,
        CancellationToken cancellationToken = default)
        => await PaymentQueries
            .ByCustomer(context.Payments.AsNoTracking(), userId, customerId)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<CheckAggregate>> GetCheckAggregatesAsync(
        long userId,
        long? customerId,
        DateTime today,
        CancellationToken cancellationToken = default)
        => await CheckQueries
            .ByCustomer(context.Checks.AsNoTracking(), userId, customerId, today)
            .ToListAsync(cancellationToken);

    public Task<int> CountCustomersAsync(
        long userId,
        CancellationToken cancellationToken = default)
        => context.Customers
            .AsNoTracking()
            .CountAsync(x => x.UserId == userId, cancellationToken);
}
