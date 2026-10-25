using Vexora.Application.Ledger.Models;

namespace Vexora.Application.Ledger.Repositories;

public interface ILedgerCustomerReader
{
    Task<IReadOnlyList<CustomerRef>> FindCustomersAsync(
        long userId,
        string? normalizedSearch,
        long? customerId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PaymentAggregate>> GetPaymentAggregatesAsync(
        long userId,
        long? customerId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CheckAggregate>> GetCheckAggregatesAsync(
        long userId,
        long? customerId,
        DateTime today,
        CancellationToken cancellationToken = default);

    Task<int> CountCustomersAsync(
        long userId,
        CancellationToken cancellationToken = default);
}
