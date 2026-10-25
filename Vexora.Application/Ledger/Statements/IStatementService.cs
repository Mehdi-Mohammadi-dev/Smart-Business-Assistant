namespace Vexora.Application.Ledger.Statements;

public interface IStatementService
{
    Task<CustomerStatementDto> GetAsync(
        long customerId,
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken = default);
}
