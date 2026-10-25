using Vexora.Application.Ledger.Models;

namespace Vexora.Application.Ledger.Repositories;

public interface IStatementReader
{
    Task<string?> GetCustomerNameAsync(
        long userId,
        long customerId,
        CancellationToken cancellationToken = default);

    Task<int> CountRowsAsync(
        long userId,
        long customerId,
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StatementPaymentRow>> GetPaymentsAsync(
        long userId,
        long customerId,
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StatementCheckRow>> GetChecksAsync(
        long userId,
        long customerId,
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken = default);
}
