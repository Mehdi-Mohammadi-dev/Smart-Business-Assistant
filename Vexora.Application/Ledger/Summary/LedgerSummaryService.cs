using Vexora.Application.Common;
using Vexora.Application.Ledger.Models;
using Vexora.Application.Ledger.Repositories;
using Vexora.Application.Services.Interfaces.CurrentUser;

namespace Vexora.Application.Ledger.Summary;

public sealed class LedgerSummaryService(
    ILedgerSummaryReader summaryReader,
    ILedgerCustomerReader customerReader,
    ICurrentUser currentUser,
    TimeProvider timeProvider)
    : ILedgerSummaryService
{
    private const int DueSoonDays = 7;

    public async Task<LedgerSummaryDto> GetAsync(
        CancellationToken cancellationToken = default)
    {
        var userId = currentUser.UserId;
        var today = IranTime.Today(timeProvider);

        var customersCount = await customerReader.CountCustomersAsync(
            userId,
            cancellationToken);

        var checks = await summaryReader.GetPendingChecksAsync(
            userId,
            today,
            DueSoonDays,
            cancellationToken)
            ?? new CheckPendingAggregate();

        var payments = await summaryReader.GetPaymentPeriodAsync(
            userId,
            IranTime.StartOfDayUtc(PersianDateFormatter.StartOfMonth(today)),
            IranTime.StartOfDayUtc(today),
            IranTime.StartOfDayUtc(today.AddDays(1)),
            cancellationToken)
            ?? new PaymentPeriodAggregate();

        return new LedgerSummaryDto(
            DateOnly.FromDateTime(today),
            PersianDateFormatter.ToPersian(today),
            customersCount,
            new CheckSummaryDto(
                checks.PendingCount,
                checks.PendingTotal,
                checks.OverdueCount,
                checks.OverdueTotal,
                checks.DueSoonCount,
                checks.DueSoonTotal,
                checks.NotRegisteredCount),
            new PaymentSummaryDto(
                payments.TodayTotal,
                payments.MonthTotal,
                payments.MonthCount));
    }
}
