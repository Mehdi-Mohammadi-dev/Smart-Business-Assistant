namespace Vexora.Application.Platform.Statements;

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);
public sealed record StatementTotalsDto(decimal CashReceived, decimal ChecksPending, decimal ChecksPaid, decimal ChecksBounced, decimal TotalReceived, int PaymentsCount, int ChecksCount);
public sealed record StatementRowDto(string Type, long Id, DateTime Date, string DatePersian, decimal Amount, string? Status, string? CheckNumber, DateTime? DueDate, string? DueDatePersian, string? BankRegistration);
public sealed record CustomerStatementDto(long CustomerId, string CustomerName, DateTime? From, DateTime? To, StatementTotalsDto Totals, IReadOnlyList<StatementRowDto> Rows, DateTimeOffset GeneratedAt, string? BusinessName = null);
