using Vexora.Domain.Enums.BankRegistrationStatuses;
using Vexora.Domain.Enums.CheckSettlementStatuses;

namespace Vexora.Application.Ledger.Checks;

public sealed record CheckListQuery(
    long? CustomerId,
    CheckSettlementStatus? Settlement,
    BankRegistrationStatus? Registration,
    DateTime? DueFrom,
    DateTime? DueTo,
    bool Overdue,
    int? DueWithinDays,
    string? Search,
    CheckSort Sort,
    int Page,
    int PageSize);
