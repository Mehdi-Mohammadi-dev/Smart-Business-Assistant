namespace Vexora.Application.Ledger.Statements;

public sealed record StatementRowDto(
    string Type,
    long Id,
    DateTime Date,
    string DatePersian,
    decimal Amount,
    string? Status,
    string? CheckNumber,
    DateTime? DueDate,
    string? DueDatePersian,
    string? BankRegistration);
