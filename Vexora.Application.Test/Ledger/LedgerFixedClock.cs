namespace Vexora.Application.Test.Ledger;

public sealed class LedgerFixedClock(DateTimeOffset utcNow) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => utcNow;
}
