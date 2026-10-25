namespace Vexora.Application.Common;

public static class IranTime
{
    private const int OffsetMinutes = 210;

    public static DateTime Now(TimeProvider timeProvider)
        => DateTime.SpecifyKind(
            timeProvider.GetUtcNow().UtcDateTime.AddMinutes(OffsetMinutes),
            DateTimeKind.Unspecified);

    public static DateTime Today(TimeProvider timeProvider)
        => Now(timeProvider).Date;
}
