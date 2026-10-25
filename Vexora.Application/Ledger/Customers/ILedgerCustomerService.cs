using Vexora.Application.Common;

namespace Vexora.Application.Ledger.Customers;

public interface ILedgerCustomerService
{
    Task<PagedResult<CustomerSummaryDto>> ListAsync(
        CustomerListQuery query,
        CancellationToken cancellationToken = default);

    Task<CustomerSummaryDto> GetAsync(
        long customerId,
        CancellationToken cancellationToken = default);
}
