using Microsoft.EntityFrameworkCore;
using Vexora.Application.Ledger.Models;
using Vexora.Application.Ledger.Queries;
using Vexora.Application.Ledger.Repositories;
using Vexora.Infrastracture.Percistance.SqlServer.EfCore.Context;

namespace Vexora.Infrastracture.Percistance.SqlServer.EfCore.Repositories.Ledger;

public sealed class LedgerSummaryReader(ApplicationDbContext context) : ILedgerSummaryReader
{
    public Task<CheckPendingAggregate?> GetPendingChecksAsync(
        long userId,
        DateTime today,
        int dueSoonDays,
        CancellationToken cancellationToken = default)
        => CheckQueries
            .PendingSummary(context.Checks.AsNoTracking(), userId, today, dueSoonDays)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<PaymentPeriodAggregate?> GetPaymentPeriodAsync(
        long userId,
        DateTimeOffset monthStartUtc,
        DateTimeOffset todayStartUtc,
        DateTimeOffset tomorrowStartUtc,
        CancellationToken cancellationToken = default)
        => PaymentQueries
            .Period(context.Payments.AsNoTracking(), userId, monthStartUtc, todayStartUtc, tomorrowStartUtc)
            .FirstOrDefaultAsync(cancellationToken);
}
