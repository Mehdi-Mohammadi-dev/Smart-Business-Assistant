namespace Vexora.Application.Ledger.Customers;

public sealed record CustomerSummaryDto(
    long Id,
    string FullName,
    decimal CashReceived,
    decimal ChecksPendingTotal,
    int ChecksPendingCount,
    int ChecksOverdueCount,
    decimal TotalReceived,
    DateTimeOffset? LastActivityAt);
