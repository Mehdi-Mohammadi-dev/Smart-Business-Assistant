using Vexora.Domain.Enums.BankRegistrationStatuses;
using Vexora.Domain.Enums.CheckSettlementStatuses;
using Vexora.Domain.Enums.TransactionDirections;

namespace Vexora.Api.Contracts.Ledger;

public sealed record CheckResponse(
    long Id,
    long CustomerId,
    string CustomerName,
    decimal Amount,
    DateTime DueDate,
    string DueDatePersian,
    string? CheckNumber,
    TransactionDirection Direction,
    CheckSettlementStatus SettlementStatus,
    BankRegistrationStatus BankRegistration,
    DateTimeOffset? PaidAt,
    DateTimeOffset? BouncedAt,
    bool IsOverdue,
    int? DaysToDue);
