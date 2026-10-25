namespace Vexora.Application.Platform.Statements;

public interface IStatementService
{
    Task<CustomerStatementDto> GetAsync(long customerId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
}
