namespace Vexora.Application.Ledger.Models;

public sealed record CustomerRef(long Id, string FullName);

public sealed record PaymentAggregate
{
    public long CustomerId { get; init; }
    public decimal Total { get; init; }
    public DateTimeOffset LastAt { get; init; }
}

public sealed record CheckAggregate
{
    public long CustomerId { get; init; }
    public decimal PendingTotal { get; init; }
    public int PendingCount { get; init; }
    public int OverdueCount { get; init; }
    public decimal PaidTotal { get; init; }
    public DateTimeOffset LastAt { get; init; }
}

public sealed record CheckPendingAggregate
{
    public int PendingCount { get; init; }
    public decimal PendingTotal { get; init; }
    public int OverdueCount { get; init; }
    public decimal OverdueTotal { get; init; }
    public int DueSoonCount { get; init; }
    public decimal DueSoonTotal { get; init; }
    public int NotRegisteredCount { get; init; }
}

public sealed record PaymentPeriodAggregate
{
    public int MonthCount { get; init; }
    public decimal MonthTotal { get; init; }
    public decimal TodayTotal { get; init; }
}
