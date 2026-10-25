using Vexora.Application.Ledger.Checks;
using Vexora.Domain.Enums.BankRegistrationStatuses;
using Vexora.Domain.Enums.CheckSettlementStatuses;

namespace Vexora.Api.Contracts.Ledger;

public sealed class CheckListRequest
{
    public long? CustomerId { get; set; }

    public CheckSettlementStatus? Settlement { get; set; }

    public BankRegistrationStatus? Registration { get; set; }

    public DateTime? DueFrom { get; set; }

    public DateTime? DueTo { get; set; }

    public bool Overdue { get; set; }

    public int? DueWithinDays { get; set; }

    public string? Search { get; set; }

    public CheckSort Sort { get; set; } = CheckSort.DueDate;

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}
