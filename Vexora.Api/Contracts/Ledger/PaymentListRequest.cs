namespace Vexora.Api.Contracts.Ledger;

public sealed class PaymentListRequest
{
    public long? CustomerId { get; set; }

    public DateTime? From { get; set; }

    public DateTime? To { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}
