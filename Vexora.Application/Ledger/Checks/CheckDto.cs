using Vexora.Domain.Enums.BankRegistrationStatuses;
using Vexora.Domain.Enums.CheckSettlementStatuses;
using Vexora.Domain.Enums.TransactionDirections;

namespace Vexora.Application.Ledger.Checks;

public sealed record CheckDto(
    long Id,
    long CustomerId,
    string CustomerName,
    decimal Amount,
    DateTime DueDate,
    string? CheckNumber,
    TransactionDirection Direction,
    CheckSettlementStatus SettlementStatus,
    BankRegistrationStatus BankRegistration,
    DateTimeOffset? PaidAt,
    DateTimeOffset? BouncedAt,
    bool IsOverdue,
    int? DaysToDue);
