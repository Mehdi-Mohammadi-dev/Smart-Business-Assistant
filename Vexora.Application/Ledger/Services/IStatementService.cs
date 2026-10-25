using Vexora.Application.Ledger.Dtos;

namespace Vexora.Application.Ledger.Services;

public interface IStatementService
{
    Task<CustomerStatementDto> GetAsync(
        long customerId,
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken = default);
}
