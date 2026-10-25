using Vexora.Application.Ledger.Models;
using Vexora.Application.Ledger.Repositories;

namespace Vexora.Application.Test.Ledger.LedgerSummaryServiceTestsHelpers;

internal sealed class FakeCustomerReader(int count) : ILedgerCustomerReader
{
    public Task<IReadOnlyList<CustomerRef>> FindCustomersAsync(long userId, string? normalizedSearch,
        long? customerId, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<CustomerRef>>([]);

    public Task<IReadOnlyList<PaymentAggregate>> GetPaymentAggregatesAsync(long userId, long? customerId,
        CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<PaymentAggregate>>([]);

    public Task<IReadOnlyList<CheckAggregate>> GetCheckAggregatesAsync(long userId, long? customerId,
        DateTime today, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<CheckAggregate>>([]);

    public Task<int> CountCustomersAsync(long userId, CancellationToken cancellationToken = default)
        => Task.FromResult(count);
}
