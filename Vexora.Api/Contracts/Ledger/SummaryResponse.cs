namespace Vexora.Api.Contracts.Ledger;

public sealed record CheckSummaryResponse(
    int PendingCount,
    decimal PendingTotal,
    int OverdueCount,
    decimal OverdueTotal,
    int DueWithin7Count,
    decimal DueWithin7Total,
    int NotRegisteredPendingCount);

public sealed record PaymentSummaryResponse(
    decimal TodayTotal,
    decimal MonthTotal,
    int MonthCount);

public sealed record SummaryResponse(
    DateOnly Today,
    string TodayPersian,
    int CustomersCount,
    CheckSummaryResponse Checks,
    PaymentSummaryResponse Payments);
