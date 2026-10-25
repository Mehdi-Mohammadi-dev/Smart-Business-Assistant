using Vexora.Application.Common;
using Vexora.Application.Ledger.Models;
using Vexora.Application.Ledger.Payments;

namespace Vexora.Application.Ledger.Repositories;

public interface ILedgerPaymentReader
{
    Task<PagedResult<PaymentRow>> ListAsync(
        long userId,
        PaymentListQuery query,
        CancellationToken cancellationToken = default);
}
