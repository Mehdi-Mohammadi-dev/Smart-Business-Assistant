using Vexora.Application.Ledger.Customers;

namespace Vexora.Api.Contracts.Ledger;

public sealed class CustomerListRequest
{
    public string? Search { get; set; }

    public CustomerSort Sort { get; set; } = CustomerSort.LastActivity;

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}
