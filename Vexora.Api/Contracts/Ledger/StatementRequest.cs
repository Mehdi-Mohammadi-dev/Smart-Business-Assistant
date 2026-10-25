namespace Vexora.Api.Contracts.Ledger;

public sealed class StatementRequest
{
    public DateTime? From { get; set; }

    public DateTime? To { get; set; }
}
