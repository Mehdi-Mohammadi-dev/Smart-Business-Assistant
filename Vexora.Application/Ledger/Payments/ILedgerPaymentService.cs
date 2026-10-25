using Vexora.Application.Common;

namespace Vexora.Application.Ledger.Payments;

public interface ILedgerPaymentService
{
    Task<PagedResult<PaymentDto>> ListAsync(
        PaymentListQuery query,
        CancellationToken cancellationToken = default);

    Task<long> DeleteAsync(
        long paymentId,
        CancellationToken cancellationToken = default);
}
