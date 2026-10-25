namespace Vexora.Application.Common;

public static class PersianDateFormatter
{
    private static readonly DateTime Anchor = new(2021, 3, 21);

    private const int AnchorYear = 1400;

    public static string ToPersian(DateTimeOffset instant)
        => ToPersian(IranTime.ToIranDateTime(instant));

    public static string ToPersian(DateTime date)
    {
        var (year, month, day) = GetParts(date);
        return $"{year:0000}/{month:00}/{day:00}";
    }

    public static (int Year, int Month, int Day) GetParts(DateTime date)
    {
        var offset = (int)(date.Date - Anchor).TotalDays;
        var year = AnchorYear;

        while (offset < 0)
        {
            year--;
            offset += DaysInYear(year);
        }

        while (offset >= DaysInYear(year))
        {
            offset -= DaysInYear(year);
            year++;
        }

        var month = 1;

        while (offset >= DaysInMonth(year, month))
        {
            offset -= DaysInMonth(year, month);
            month++;
        }

        return (year, month, offset + 1);
    }

    public static DateTime StartOfMonth(DateTime date)
    {
        var (_, _, day) = GetParts(date);
        return date.Date.AddDays(-(day - 1));
    }

    public static bool IsLeapYear(int year)
        => (year * 25 + 11) % 33 < 8;

    private static int DaysInYear(int year)
        => IsLeapYear(year) ? 366 : 365;

    private static int DaysInMonth(int year, int month)
    {
        if (month <= 6)
            return 31;

        if (month <= 11)
            return 30;

        return IsLeapYear(year) ? 30 : 29;
    }
}
