using System.Globalization;
using System.Text.RegularExpressions;
using Vexora.Application.NLU.Abstractions.Dates;

namespace Vexora.Application.NLU.Extractions.Dates;

public sealed class PersianDateExtractor : IDateExtractor
{
    private static readonly PersianCalendar PersianCalendar = new();

    private static readonly string[] PersianMonths =
    [
        "فروردین",
        "اردیبهشت",
        "خرداد",
        "تیر",
        "مرداد",
        "شهریور",
        "مهر",
        "آبان",
        "آذر",
        "دی",
        "بهمن",
        "اسفند"
    ];

    public DateTime? Extract(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        text = Normalize(text);

        var relativeDate = ExtractRelativeDate(text);

        if (relativeDate.HasValue)
            return relativeDate;

        var numericDate = ExtractNumericDate(text);

        if (numericDate.HasValue)
            return numericDate;

        return ExtractPersianMonthDate(text);
    }

    private static DateTime? ExtractRelativeDate(string text)
    {
        if (text.Contains("پس فردا"))
            return DateTime.Today.AddDays(2);

        if (text.Contains("فردا"))
            return DateTime.Today.AddDays(1);

        return null;
    }

    private static DateTime? ExtractNumericDate(string text)
    {
        var match = Regex.Match(
            text,
            @"(?<year>\d{4})[\/\-](?<month>\d{1,2})[\/\-](?<day>\d{1,2})");

        if (match.Success)
        {
            var year = int.Parse(match.Groups["year"].Value);
            var month = int.Parse(match.Groups["month"].Value);
            var day = int.Parse(match.Groups["day"].Value);

            return TryCreatePersianDate(
                year,
                month,
                day);
        }

        match = Regex.Match(
            text,
            @"(?<day>\d{1,2})[\/\-](?<month>\d{1,2})");

        if (!match.Success)
            return null;

        var currentPersianYear =
            PersianCalendar.GetYear(DateTime.Today);

        var currentMonth =
            int.Parse(match.Groups["month"].Value);

        var currentDay =
            int.Parse(match.Groups["day"].Value);

        return TryCreatePersianDate(
            currentPersianYear,
            currentMonth,
            currentDay);
    }

    private static DateTime? ExtractPersianMonthDate(string text)
    {
        var monthPattern = string.Join(
            "|",
            PersianMonths.Select(Regex.Escape));

        var match = Regex.Match(
            text,
            $@"(?<day>\d{{1,2}})\s+(?<month>{monthPattern})(?:\s+(?<year>\d{{4}}))?");

        if (!match.Success)
            return null;

        var day = int.Parse(match.Groups["day"].Value);

        var monthName =
            match.Groups["month"].Value;

        var month =
            Array.IndexOf(
                PersianMonths,
                monthName) + 1;

        var year =
            match.Groups["year"].Success
                ? int.Parse(match.Groups["year"].Value)
                : PersianCalendar.GetYear(DateTime.Today);

        return TryCreatePersianDate(
            year,
            month,
            day);
    }

    private static DateTime? TryCreatePersianDate(
        int year,
        int month,
        int day)
    {
        try
        {
            return PersianCalendar.ToDateTime(
                year,
                month,
                day,
                0,
                0,
                0,
                0);
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
    }

    private static string Normalize(string text)
    {
        return text
            .Trim()
            .Replace('ي', 'ی')
            .Replace('ك', 'ک')
            .Replace('۰', '0')
            .Replace('۱', '1')
            .Replace('۲', '2')
            .Replace('۳', '3')
            .Replace('۴', '4')
            .Replace('۵', '5')
            .Replace('۶', '6')
            .Replace('۷', '7')
            .Replace('۸', '8')
            .Replace('۹', '9');
    }
}