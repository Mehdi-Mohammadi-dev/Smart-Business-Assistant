namespace Vexora.Application.Ledger.Statements;

public sealed record StatementTotalsDto(
    decimal CashReceived,
    decimal ChecksPending,
    decimal ChecksPaid,
    decimal ChecksBounced,
    decimal TotalReceived,
    int PaymentsCount,
    int ChecksCount);
