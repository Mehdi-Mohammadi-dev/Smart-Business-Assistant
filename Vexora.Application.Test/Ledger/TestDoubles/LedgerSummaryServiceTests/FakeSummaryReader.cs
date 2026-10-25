using Vexora.Application.Ledger.Models;
using Vexora.Application.Ledger.Repositories;

namespace Vexora.Application.Test.Ledger.LedgerSummaryServiceTestsHelpers;

internal sealed class FakeSummaryReader(CheckPendingAggregate? checks, PaymentPeriodAggregate? payments)
    : ILedgerSummaryReader
{
    public DateTime SeenToday;
    public int SeenDueSoonDays;
    public (DateTimeOffset Month, DateTimeOffset Today, DateTimeOffset Tomorrow) SeenBounds;

    public Task<CheckPendingAggregate?> GetPendingChecksAsync(long userId, DateTime today, int dueSoonDays,
        CancellationToken cancellationToken = default)
    {
        SeenToday = today;
        SeenDueSoonDays = dueSoonDays;
        return Task.FromResult(checks);
    }

    public Task<PaymentPeriodAggregate?> GetPaymentPeriodAsync(long userId, DateTimeOffset monthStartUtc,
        DateTimeOffset todayStartUtc, DateTimeOffset tomorrowStartUtc, CancellationToken cancellationToken = default)
    {
        SeenBounds = (monthStartUtc, todayStartUtc, tomorrowStartUtc);
        return Task.FromResult(payments);
    }
}
