namespace Vexora.Application.Common;

public static class IranTime
{
    public static readonly TimeSpan Offset = new(3, 30, 0);

    public static DateTime Today(TimeProvider timeProvider)
        => ToIranDateTime(timeProvider.GetUtcNow()).Date;

    public static DateTime ToIranDateTime(DateTimeOffset value)
        => value.ToOffset(Offset).DateTime;

    public static DateTime ToIranDate(DateTimeOffset value)
        => ToIranDateTime(value).Date;

    public static DateTimeOffset StartOfDayUtc(DateTime iranDate)
        => new DateTimeOffset(DateTime.SpecifyKind(iranDate.Date, DateTimeKind.Unspecified), Offset)
            .ToUniversalTime();
}
