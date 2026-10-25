using Vexora.Application.Ledger.Models;

namespace Vexora.Application.Ledger.Repositories;

public interface ILedgerSummaryReader
{
    Task<CheckPendingAggregate?> GetPendingChecksAsync(
        long userId,
        DateTime today,
        int dueSoonDays,
        CancellationToken cancellationToken = default);

    Task<PaymentPeriodAggregate?> GetPaymentPeriodAsync(
        long userId,
        DateTimeOffset monthStartUtc,
        DateTimeOffset todayStartUtc,
        DateTimeOffset tomorrowStartUtc,
        CancellationToken cancellationToken = default);
}
