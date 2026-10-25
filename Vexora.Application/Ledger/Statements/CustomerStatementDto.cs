namespace Vexora.Application.Ledger.Statements;

public sealed record CustomerStatementDto(
    long CustomerId,
    string CustomerName,
    DateTime? From,
    DateTime? To,
    StatementTotalsDto Totals,
    IReadOnlyList<StatementRowDto> Rows,
    DateTimeOffset GeneratedAt);
