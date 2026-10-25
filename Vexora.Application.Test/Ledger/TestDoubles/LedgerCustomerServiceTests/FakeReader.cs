using FluentAssertions;
using Vexora.Application.Ledger.Models;
using Vexora.Application.Ledger.Repositories;

namespace Vexora.Application.Test.Ledger.LedgerCustomerServiceTestsHelpers;

internal sealed class FakeReader : ILedgerCustomerReader
{
    public readonly Dictionary<long, List<CustomerRef>> CustomersByUser = new();
    public string? LastSearch;

    public Task<IReadOnlyList<CustomerRef>> FindCustomersAsync(
        long userId, string? normalizedSearch, long? customerId, CancellationToken cancellationToken = default)
    {
        LastSearch = normalizedSearch;

        var list = CustomersByUser.GetValueOrDefault(userId) ?? [];

        IEnumerable<CustomerRef> query = list;

        if (customerId is { } id)
            query = query.Where(x => x.Id == id);

        if (normalizedSearch is not null)
            query = query.Where(x => x.FullName.Contains(normalizedSearch));

        return Task.FromResult<IReadOnlyList<CustomerRef>>(query.ToList());
    }

    public Task<IReadOnlyList<PaymentAggregate>> GetPaymentAggregatesAsync(
        long userId, long? customerId, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<PaymentAggregate>>([]);

    public Task<IReadOnlyList<CheckAggregate>> GetCheckAggregatesAsync(
        long userId, long? customerId, DateTime today, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<CheckAggregate>>([]);

    public Task<int> CountCustomersAsync(long userId, CancellationToken cancellationToken = default)
        => Task.FromResult(CustomersByUser.GetValueOrDefault(userId)?.Count ?? 0);
}
