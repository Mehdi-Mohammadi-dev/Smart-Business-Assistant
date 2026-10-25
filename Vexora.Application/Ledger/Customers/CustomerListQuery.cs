namespace Vexora.Application.Ledger.Customers;

public sealed record CustomerListQuery(
    string? Search,
    CustomerSort Sort,
    int Page,
    int PageSize);
