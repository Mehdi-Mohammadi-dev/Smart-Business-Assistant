namespace Vexora.Api.Contracts.Ledger;

public sealed record CustomerSummaryResponse(
    long Id,
    string FullName,
    decimal CashReceived,
    decimal ChecksPendingTotal,
    int ChecksPendingCount,
    int ChecksOverdueCount,
    decimal TotalReceived,
    DateTimeOffset? LastActivityAt);
