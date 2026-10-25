using Vexora.Application.Common;

namespace Vexora.Application.Ledger.Queries;

public readonly record struct DateRangeBounds(
    DateTimeOffset? FromUtc,
    DateTimeOffset? ToUtcExclusive)
{
    public static DateRangeBounds FromIranDays(DateTime? from, DateTime? to)
        => new(
            from.HasValue ? IranStartOfDay(from.Value) : null,
            to.HasValue ? IranStartOfDay(to.Value.Date.AddDays(1)) : null);

    private static DateTimeOffset IranStartOfDay(DateTime day)
        => IranTime.StartOfDayUtc(day);
}
