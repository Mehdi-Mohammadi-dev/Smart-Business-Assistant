using System.Globalization;

namespace Vexora.Application.Common;

public static class PersianDateFormatter
{
    private static readonly PersianCalendar Calendar = new();

    public static string ToPersian(DateTime date)
        => Format(date);

    public static string ToPersian(DateTimeOffset date)
        => Format(date.UtcDateTime.AddMinutes(210));

    private static string Format(DateTime date)
    {
        var year = Calendar.GetYear(date);
        var month = Calendar.GetMonth(date);
        var day = Calendar.GetDayOfMonth(date);

        return $"{year:0000}/{month:00}/{day:00}";
    }
}
