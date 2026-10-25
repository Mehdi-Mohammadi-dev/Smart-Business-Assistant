using Vexora.Application.Ledger.Models;
using Vexora.Application.Ledger.Repositories;

namespace Vexora.Application.Test.Ledger.StatementTestsHelpers;

internal sealed class FakeStatementReader(int count, string? name = "علی رضایی") : IStatementReader
{
    public long? SeenUserId;

    public Task<string?> GetCustomerNameAsync(long userId, long customerId, CancellationToken cancellationToken = default)
    {
        SeenUserId = userId;
        return Task.FromResult(name);
    }

    public Task<int> CountRowsAsync(long userId, long customerId, DateTime? from, DateTime? to,
        CancellationToken cancellationToken = default)
        => Task.FromResult(count);

    public Task<IReadOnlyList<StatementPaymentRow>> GetPaymentsAsync(long userId, long customerId,
        DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<StatementPaymentRow>>([]);

    public Task<IReadOnlyList<StatementCheckRow>> GetChecksAsync(long userId, long customerId,
        DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<StatementCheckRow>>([]);
}
