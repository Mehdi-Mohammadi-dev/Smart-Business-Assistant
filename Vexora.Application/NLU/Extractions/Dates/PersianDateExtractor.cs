using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Vexora.Application.NLU.Abstractions.Dates;
using Vexora.Application.NLU.Intent;
using Vexora.Application.NLU.Parsing.Number;

namespace Vexora.Application.NLU.Extractions.Dates;

public sealed partial class PersianDateExtractor : IDateExtractor
{
    private const int IranOffsetMinutes = 210;
    private const int PastToleranceDays = 60;

    private static readonly PersianCalendar Calendar = new();

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

    private static readonly Dictionary<string, int> OrdinalDays = new(StringComparer.Ordinal)
    {
        ["اول"] = 1, ["یکم"] = 1, ["دوم"] = 2, ["سوم"] = 3, ["چهارم"] = 4, ["پنجم"] = 5,
        ["ششم"] = 6, ["هفتم"] = 7, ["هشتم"] = 8, ["نهم"] = 9, ["دهم"] = 10,
        ["یازدهم"] = 11, ["دوازدهم"] = 12, ["سیزدهم"] = 13, ["چهاردهم"] = 14,
        ["پانزدهم"] = 15, ["شانزدهم"] = 16, ["هفدهم"] = 17, ["هجدهم"] = 18,
        ["نوزدهم"] = 19, ["بیستم"] = 20, ["سیم"] = 30, ["سیام"] = 30
    };

    private static readonly Dictionary<string, int> UnitOrdinals = new(StringComparer.Ordinal)
    {
        ["یکم"] = 1, ["دوم"] = 2, ["سوم"] = 3, ["چهارم"] = 4, ["پنجم"] = 5,
        ["ششم"] = 6, ["هفتم"] = 7, ["هشتم"] = 8, ["نهم"] = 9
    };

    private static readonly Dictionary<string, int> TensBeforeAnd = new(StringComparer.Ordinal)
    {
        ["بیست"] = 20,
        ["سی"] = 30
    };

    private static readonly Dictionary<string, int> Weekdays = new(StringComparer.Ordinal)
    {
        ["شنبه"] = 0, ["یکشنبه"] = 1, ["دوشنبه"] = 2, ["سهشنبه"] = 3,
        ["چهارشنبه"] = 4, ["پنجشنبه"] = 5, ["جمعه"] = 6
    };

    private static readonly Dictionary<string, int> WeekdayNumberPrefix = new(StringComparer.Ordinal)
    {
        ["یک"] = 1, ["دو"] = 2, ["سه"] = 3, ["چهار"] = 4, ["پنج"] = 5
    };

    private static readonly HashSet<string> NextWords = ["دیگه", "دیگر", "بعد", "بعدی", "آینده"];

    private static readonly HashSet<string> DurationUnits = ["روز", "هفته", "ماه", "سال"];

    private static readonly HashSet<string> MoneyWords =
    [
        "تومان", "تومن", "ریال", "هزار", "میلیون", "میلیارد", "تومانی", "تومنی", "ریالی"
    ];

    private readonly TimeProvider _timeProvider;
    private readonly PersianNumberParser _numberParser = new();

    public PersianDateExtractor(TimeProvider? timeProvider = null)
    {
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    private DateTime Today =>
        _timeProvider.GetUtcNow().UtcDateTime.AddMinutes(IranOffsetMinutes).Date;

    public DateTime? Extract(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        text = Normalize(text);

        var tokens = PhraseMatcher.Tokenize(text);

        DateTime? result;

        if (TryExtractFullNumeric(text, out result) ||
            TryExtractDayAndMonth(tokens, out result) ||
            TryExtractWeekday(tokens, out result) ||
            TryExtractMonthBoundary(tokens, out result) ||
            TryExtractDuration(tokens, out result) ||
            TryExtractTomorrow(tokens, out result) ||
            TryExtractShortNumeric(text, out result))
        {
            return result is null
                ? null
                : DateTime.SpecifyKind(result.Value.Date, DateTimeKind.Unspecified);
        }

        return null;
    }

    private static bool TryExtractFullNumeric(string text, out DateTime? date)
    {
        date = null;

        var match = FullNumericRegex().Match(text);

        if (!match.Success)
            return false;

        var year = int.Parse(match.Groups["year"].Value);
        var month = int.Parse(match.Groups["month"].Value);
        var day = int.Parse(match.Groups["day"].Value);

        if (year is >= 1300 and <= 1499)
        {
            date = TryCreatePersianDate(year, month, day);
            return true;
        }

        if (year is >= 1700 and <= 2300)
        {
            try
            {
                date = new DateTime(year, month, day);
            }
            catch (ArgumentOutOfRangeException)
            {
                date = null;
            }

            return true;
        }

        return false;
    }

    private bool TryExtractDayAndMonth(IReadOnlyList<string> tokens, out DateTime? date)
    {
        date = null;

        for (var i = 0; i < tokens.Count; i++)
        {
            var month = MonthNumber(tokens[i]);

            if (month == 0)
                continue;

            var dayIndex = i - 1;

            if (dayIndex >= 0 && tokens[dayIndex] == "ماه")
                dayIndex--;

            if (dayIndex < 0 || !TryParseDay(tokens, dayIndex, out var day))
                continue;

            var yearIndex = i + 1;

            if (yearIndex < tokens.Count && tokens[yearIndex] == "ماه")
                yearIndex++;

            if (TryReadYear(tokens, yearIndex, out var year))
            {
                date = TryCreatePersianDate(year, month, day);
                return true;
            }

            date = ResolveWithoutYear(month, day);
            return true;
        }

        return false;
    }

    private static bool TryReadYear(IReadOnlyList<string> tokens, int index, out int year)
    {
        year = 0;

        if (index >= tokens.Count || !YearRegex().IsMatch(tokens[index]))
            return false;

        if (index + 1 < tokens.Count && MoneyWords.Contains(tokens[index + 1]))
            return false;

        year = int.Parse(tokens[index]);
        return true;
    }

    private static bool TryParseDay(IReadOnlyList<string> tokens, int index, out int day)
    {
        day = 0;

        var token = tokens[index];

        var digits = DayDigitsRegex().Match(token);

        if (digits.Success)
        {
            day = int.Parse(digits.Groups["day"].Value);
            return day is >= 1 and <= 31;
        }

        if (token == "ام" && index > 0)
        {
            var previous = tokens[index - 1];

            if (DayOnlyDigitsRegex().IsMatch(previous))
            {
                day = int.Parse(previous);
                return day is >= 1 and <= 31;
            }

            if (TensBeforeAnd.TryGetValue(previous, out var tens))
            {
                day = tens;
                return true;
            }
        }

        if (UnitOrdinals.TryGetValue(token, out var unit) &&
            index >= 2 &&
            tokens[index - 1] == "و" &&
            TensBeforeAnd.TryGetValue(tokens[index - 2], out var tensValue))
        {
            day = tensValue + unit;
            return day <= 31;
        }

        if (OrdinalDays.TryGetValue(token, out day))
            return true;

        return false;
    }

    private DateTime? ResolveWithoutYear(int month, int day)
    {
        var today = Today;
        var year = Calendar.GetYear(today);

        var candidate = TryCreatePersianDate(year, month, day);

        if (candidate is not null && candidate.Value >= today.AddDays(-PastToleranceDays))
            return candidate;

        var nextYear = TryCreatePersianDate(year + 1, month, day);

        return candidate is null ? nextYear : nextYear ?? candidate;
    }

    private bool TryExtractWeekday(IReadOnlyList<string> tokens, out DateTime? date)
    {
        date = null;

        for (var i = 0; i < tokens.Count; i++)
        {
            if (!TryReadWeekday(tokens, i, out var weekday, out var lastIndex))
                continue;

            var today = Today;
            var todayIndex = WeekIndex(today);
            var weekStart = today.AddDays(-todayIndex);

            var after1 = lastIndex + 1 < tokens.Count ? tokens[lastIndex + 1] : string.Empty;
            var after2 = lastIndex + 2 < tokens.Count ? tokens[lastIndex + 2] : string.Empty;

            if (after1 == "هفته" && NextWords.Contains(after2))
            {
                date = weekStart.AddDays(7 + weekday);
                return true;
            }

            if (after1 == "این" && after2 == "هفته")
            {
                date = weekStart.AddDays(weekday);
                return true;
            }

            var delta = (weekday - todayIndex + 7) % 7;

            if (delta == 0)
                delta = 7;

            date = today.AddDays(delta);
            return true;
        }

        return false;
    }

    private static bool TryReadWeekday(
        IReadOnlyList<string> tokens,
        int index,
        out int weekday,
        out int lastIndex)
    {
        weekday = 0;
        lastIndex = index;

        var token = tokens[index];

        if (token == "شنبه")
        {
            if (index > 0 && WeekdayNumberPrefix.TryGetValue(tokens[index - 1], out var number))
            {
                weekday = number;
                return true;
            }

            weekday = 0;
            return true;
        }

        if (Weekdays.TryGetValue(token, out weekday))
            return true;

        return false;
    }

    private static int WeekIndex(DateTime date)
        => ((int)date.DayOfWeek + 1) % 7;

    private bool TryExtractMonthBoundary(IReadOnlyList<string> tokens, out DateTime? date)
    {
        date = null;

        for (var i = 0; i < tokens.Count; i++)
        {
            if (tokens[i] == "آخر")
            {
                var k = i + 1;

                if (k < tokens.Count && tokens[k] == "این")
                    k++;

                if (k >= tokens.Count || tokens[k] != "ماه")
                    continue;

                var following = k + 1 < tokens.Count ? tokens[k + 1] : string.Empty;

                if (MonthNumber(following) > 0)
                {
                    date = LastDayOfNamedMonth(MonthNumber(following));
                    return true;
                }

                date = LastDayOfMonth(NextWords.Contains(following) ? 1 : 0);
                return true;
            }

            if (tokens[i] == "اول" &&
                i + 2 < tokens.Count &&
                tokens[i + 1] == "ماه" &&
                NextWords.Contains(tokens[i + 2]))
            {
                var nextMonth = Calendar.AddMonths(Today, 1);

                date = TryCreatePersianDate(
                    Calendar.GetYear(nextMonth),
                    Calendar.GetMonth(nextMonth),
                    1);

                return true;
            }
        }

        return false;
    }

    private DateTime? LastDayOfMonth(int monthsFromNow)
    {
        var target = Calendar.AddMonths(Today, monthsFromNow);
        var year = Calendar.GetYear(target);
        var month = Calendar.GetMonth(target);

        return TryCreatePersianDate(year, month, Calendar.GetDaysInMonth(year, month));
    }

    private DateTime? LastDayOfNamedMonth(int month)
    {
        var today = Today;
        var year = Calendar.GetYear(today);

        var candidate = TryCreatePersianDate(year, month, Calendar.GetDaysInMonth(year, month));

        if (candidate is not null && candidate.Value >= today.AddDays(-PastToleranceDays))
            return candidate;

        return TryCreatePersianDate(year + 1, month, Calendar.GetDaysInMonth(year + 1, month));
    }

    private bool TryExtractDuration(IReadOnlyList<string> tokens, out DateTime? date)
    {
        date = null;

        for (var i = 0; i + 1 < tokens.Count; i++)
        {
            if (!DurationUnits.Contains(tokens[i]) || !NextWords.Contains(tokens[i + 1]))
                continue;

            var quantity = ReadQuantity(tokens, i);

            date = AddDuration(tokens[i], quantity);
            return true;
        }

        return false;
    }

    private int ReadQuantity(IReadOnlyList<string> tokens, int unitIndex)
    {
        for (var start = Math.Max(0, unitIndex - 4); start < unitIndex; start++)
        {
            var words = tokens.Skip(start).Take(unitIndex - start).ToList();

            if (words.Any(MoneyWords.Contains))
                continue;

            var phrase = string.Join(' ', words);

            if (DayOnlyDigitsRegex().IsMatch(phrase) || NumberOnlyDigitsRegex().IsMatch(phrase))
                return int.Parse(phrase);

            if (_numberParser.TryParse(phrase, out var parsed) &&
                parsed is not null &&
                parsed.Value > 0)
            {
                return (int)parsed.Value;
            }
        }

        return 1;
    }

    private DateTime? AddDuration(string unit, int quantity)
    {
        var today = Today;

        switch (unit)
        {
            case "روز" when quantity <= 1100:
                return today.AddDays(quantity);

            case "هفته" when quantity <= 160:
                return today.AddDays(7 * quantity);

            case "ماه" when quantity <= 36:
                return Calendar.AddMonths(today, quantity);

            case "سال" when quantity <= 3:
                return Calendar.AddYears(today, quantity);

            default:
                return null;
        }
    }

    private bool TryExtractTomorrow(IReadOnlyList<string> tokens, out DateTime? date)
    {
        date = null;

        if (PhraseMatcher.ContainsPhrase(tokens, "پس فردا") || tokens.Contains("پسفردا"))
        {
            date = Today.AddDays(2);
            return true;
        }

        if (tokens.Contains("فردا"))
        {
            date = Today.AddDays(1);
            return true;
        }

        return false;
    }

    private bool TryExtractShortNumeric(string text, out DateTime? date)
    {
        date = null;

        var match = ShortNumericRegex().Match(text);

        if (!match.Success)
            return false;

        date = ResolveWithoutYear(
            int.Parse(match.Groups["month"].Value),
            int.Parse(match.Groups["day"].Value));

        return true;
    }

    private static int MonthNumber(string token)
        => Array.IndexOf(PersianMonths, token) + 1;

    private static DateTime? TryCreatePersianDate(int year, int month, int day)
    {
        if (month is < 1 or > 12 || day is < 1 or > 31)
            return null;

        try
        {
            return Calendar.ToDateTime(year, month, day, 0, 0, 0, 0);
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
    }

    private static string Normalize(string text)
    {
        var builder = new StringBuilder(text.Length);

        foreach (var ch in text)
        {
            switch (ch)
            {
                case >= '۰' and <= '۹':
                    builder.Append((char)('0' + (ch - '۰')));
                    break;

                case >= '٠' and <= '٩':
                    builder.Append((char)('0' + (ch - '٠')));
                    break;

                case 'ي':
                    builder.Append('ی');
                    break;

                case 'ك':
                    builder.Append('ک');
                    break;

                case '\u200c':
                    builder.Append(' ');
                    break;

                case 'ـ':
                    break;

                default:
                    builder.Append(ch);
                    break;
            }
        }

        var normalized = WhitespaceRegex().Replace(builder.ToString(), " ").Trim();

        return normalized
            .Replace("سه شنبه", "سهشنبه")
            .Replace("چهار شنبه", "چهارشنبه")
            .Replace("پنج شنبه", "پنجشنبه")
            .Replace("دو شنبه", "دوشنبه")
            .Replace("یک شنبه", "یکشنبه");
    }

    [GeneratedRegex(@"(?<![\d/\-])(?<year>\d{4})[/\-](?<month>\d{1,2})[/\-](?<day>\d{1,2})(?!\d)")]
    private static partial Regex FullNumericRegex();

    [GeneratedRegex(@"(?<![\d/\-])(?<day>\d{1,2})[/\-](?<month>\d{1,2})(?![\d/\-])")]
    private static partial Regex ShortNumericRegex();

    [GeneratedRegex(@"^(?<day>\d{1,2})(?:ام|م)?$")]
    private static partial Regex DayDigitsRegex();

    [GeneratedRegex(@"^\d{1,2}$")]
    private static partial Regex DayOnlyDigitsRegex();

    [GeneratedRegex(@"^\d{1,4}$")]
    private static partial Regex NumberOnlyDigitsRegex();

    [GeneratedRegex(@"^1[34]\d{2}$")]
    private static partial Regex YearRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}
