using Vexora.Domain.Enums.BankRegistrationStatuses;
using Vexora.Domain.Enums.CheckSettlementStatuses;
using Vexora.Domain.Enums.TransactionDirections;

namespace Vexora.Application.Ledger.Models;

public sealed record PaymentRow
{
    public long Id { get; init; }
    public long CustomerId { get; init; }
    public string CustomerFirstName { get; init; } = string.Empty;
    public string CustomerLastName { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public DateTimeOffset PaymentDate { get; init; }
    public TransactionDirection Direction { get; init; }
}

public sealed record CheckRow
{
    public long Id { get; init; }
    public long CustomerId { get; init; }
    public string CustomerFirstName { get; init; } = string.Empty;
    public string CustomerLastName { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public DateTime DueDate { get; init; }
    public string? CheckNumber { get; init; }
    public TransactionDirection Direction { get; init; }
    public CheckSettlementStatus SettlementStatus { get; init; }
    public BankRegistrationStatus BankRegistration { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? PaidAt { get; init; }
    public DateTimeOffset? BouncedAt { get; init; }
}

public sealed record StatementPaymentRow
{
    public long Id { get; init; }
    public DateTimeOffset PaymentDate { get; init; }
    public decimal Amount { get; init; }
}

public sealed record StatementCheckRow
{
    public long Id { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public decimal Amount { get; init; }
    public DateTime DueDate { get; init; }
    public string? CheckNumber { get; init; }
    public CheckSettlementStatus SettlementStatus { get; init; }
    public BankRegistrationStatus BankRegistration { get; init; }
}
