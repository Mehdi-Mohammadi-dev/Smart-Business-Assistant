namespace Vexora.Application.Common;

public static class IranTime
{
    public static readonly TimeSpan Offset = new(3, 30, 0);

    /// <summary>Current wall-clock time in Iran, with an unspecified DateTime kind.</summary>
    public static DateTime Now(TimeProvider timeProvider)
        => DateTime.SpecifyKind(ToIranDateTime(timeProvider.GetUtcNow()), DateTimeKind.Unspecified);

    public static DateTime Today(TimeProvider timeProvider)
        => Now(timeProvider).Date;

    public static DateTime ToIranDateTime(DateTimeOffset value)
        => value.ToOffset(Offset).DateTime;

    public static DateTime ToIranDate(DateTimeOffset value)
        => ToIranDateTime(value).Date;

    public static DateTimeOffset StartOfDayUtc(DateTime iranDate)
        => new DateTimeOffset(
            DateTime.SpecifyKind(iranDate.Date, DateTimeKind.Unspecified),
            Offset).ToUniversalTime();
}
