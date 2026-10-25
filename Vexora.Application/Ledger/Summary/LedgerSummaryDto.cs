namespace Vexora.Application.Ledger.Summary;

public sealed record CheckSummaryDto(
    int PendingCount,
    decimal PendingTotal,
    int OverdueCount,
    decimal OverdueTotal,
    int DueWithin7Count,
    decimal DueWithin7Total,
    int NotRegisteredPendingCount);

public sealed record PaymentSummaryDto(
    decimal TodayTotal,
    decimal MonthTotal,
    int MonthCount);

public sealed record LedgerSummaryDto(
    DateOnly Today,
    string TodayPersian,
    int CustomersCount,
    CheckSummaryDto Checks,
    PaymentSummaryDto Payments);
