using System.Text.RegularExpressions;
using Vexora.Application.Dtos.Customers;
using Vexora.Application.NLU.Abstractions.Extractions.Customers;
using Vexora.Application.NLU.Models.Entities;
using Vexora.Application.NLU.Normalizations.Parsing.Number;

namespace Vexora.Application.NLU.Extractions.Customers;

public sealed partial class CustomerExtractor : ICustomerExtractor
{
    private const int MaxNameWords = 3;
    private const int MinWordLength = 2;

    private const double ScoreAfterFrom = 5;
    private const double ScoreAfterCheck = 4;
    private const double ScoreFirstRun = 3;
    private const double ScoreAfterMoney = 2;
    private const double ScoreAfterToOrFor = 1.5;
    private const double ScoreOther = 0.5;

    private static readonly HashSet<string> Honorifics =
    [
        "آقای",
        "اقای",
        "خانم",
        "خانوم",
        "جناب",
        "سرکار"
    ];

    private static readonly HashSet<string> MoneyWords =
    [
        "هزار",
        "میلیون",
        "میلیونی",
        "میلیارد",
        "میلیاردی",
        "تومان",
        "تومن",
        "تومانی",
        "تومنی",
        "ریال",
        "ریالی",
        "نیم",
        "ربع"
    ];

    private static readonly HashSet<string> MonthNames =
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

    private static readonly HashSet<string> OrdinalWords =
    [
        "اول", "دوم", "سوم", "چهارم", "پنجم", "ششم", "هفتم", "هشتم", "نهم", "دهم",
        "یازدهم", "دوازدهم", "سیزدهم", "چهاردهم", "پانزدهم", "شانزدهم", "هفدهم",
        "هجدهم", "نوزدهم", "بیستم", "سی‌ام", "سیم", "ام"
    ];

    private static readonly HashSet<string> StopWords = BuildStopWords();

    public EntityType EntityType => EntityType.Customer;

    public IReadOnlyCollection<EntityResult> Extract(
        string normalizedText)
    {
        if (string.IsNullOrWhiteSpace(normalizedText))
            return [];

        var tokens = Tokenize(normalizedText);

        var runs = FindRuns(tokens);

        if (runs.Count == 0)
            return [];

        var best = runs
            .OrderByDescending(run => run.Score)
            .ThenBy(run => run.Start)
            .First();

        var name = normalizedText
            .Substring(best.Start, best.End - best.Start)
            .Trim();

        var confidence = ToConfidence(best.Score);

        return
        [
            new EntityResult(
                EntityType.Customer,
                name,
                new CustomerMention(
                    name,
                    best.Start,
                    best.End - best.Start,
                    confidence),
                best.Start,
                best.End - best.Start,
                confidence)
        ];
    }

    private static List<NameRun> FindRuns(
        IReadOnlyList<Token> tokens)
    {
        var runs = new List<NameRun>();

        var index = 0;

        while (index < tokens.Count)
        {
            if (!IsNameWord(tokens, index))
            {
                index++;
                continue;
            }

            var first = index;
            var count = 0;

            while (index < tokens.Count &&
                   count < MaxNameWords &&
                   IsNameWord(tokens, index))
            {
                index++;
                count++;
            }

            var last = index - 1;

            runs.Add(new NameRun(
                tokens[first].Start,
                tokens[last].End,
                ScoreRun(tokens, first, runs.Count == 0)));

            while (index < tokens.Count && IsNameWord(tokens, index))
                index++;
        }

        return runs;
    }

    private static double ScoreRun(
        IReadOnlyList<Token> tokens,
        int firstTokenIndex,
        bool isFirstRun)
    {
        var previous = PreviousMeaningfulToken(tokens, firstTokenIndex);

        var score = ScoreOther;

        if (previous is null)
            return Math.Max(score, ScoreFirstRun);

        if (previous.Text == "از")
            score = Math.Max(score, ScoreAfterFrom);

        if (previous.Text is "چک" or "چکی" or "چکها")
            score = Math.Max(score, ScoreAfterCheck);

        if (previous.IsNumber || MoneyWords.Contains(previous.Text))
            score = Math.Max(score, ScoreAfterMoney);

        if (previous.Text is "به" or "برای")
            score = Math.Max(score, ScoreAfterToOrFor);

        if (isFirstRun)
            score = Math.Max(score, ScoreFirstRun);

        return score;
    }

    private static Token? PreviousMeaningfulToken(
        IReadOnlyList<Token> tokens,
        int index)
    {
        for (var i = index - 1; i >= 0; i--)
        {
            if (Honorifics.Contains(tokens[i].Text))
                continue;

            return tokens[i];
        }

        return null;
    }

    private static bool IsNameWord(
        IReadOnlyList<Token> tokens,
        int index)
    {
        var token = tokens[index];

        if (token.IsNumber || token.Text.Length < MinWordLength)
            return false;

        if (Honorifics.Contains(token.Text) || StopWords.Contains(token.Text))
            return false;

        if (MonthNames.Contains(token.Text) && IsPartOfDate(tokens, index))
            return false;

        return true;
    }

    private static bool IsPartOfDate(
        IReadOnlyList<Token> tokens,
        int index)
    {
        if (index > 0 &&
            (tokens[index - 1].IsNumber || OrdinalWords.Contains(tokens[index - 1].Text)))
            return true;

        return index + 1 < tokens.Count && tokens[index + 1].Text == "ماه";
    }

    private static List<Token> Tokenize(string text)
    {
        return TokenRegex()
            .Matches(text)
            .Select(match => new Token(
                match.Value,
                match.Index,
                match.Index + match.Length,
                char.IsDigit(match.Value[0])))
            .ToList();
    }

    private static double ToConfidence(double score) => score switch
    {
        >= ScoreAfterFrom => 0.90,
        >= ScoreAfterCheck => 0.85,
        >= ScoreFirstRun => 0.80,
        >= ScoreAfterMoney => 0.70,
        _ => 0.60
    };

    private static HashSet<string> BuildStopWords()
    {
        var words = new HashSet<string>(StringComparer.Ordinal)
        {
            "از", "به", "برای", "با", "در", "بر", "تا", "و", "که", "را", "رو", "هم", "هر",
            "این", "آن", "اون", "یه", "یک", "بابت", "طی", "طبق", "مثل", "مثلا", "مثلاً",
            "فقط", "حدود", "حدودا", "من", "ما", "او", "ایشان", "ایشون", "بهم", "بهش",
            "برام", "ازش", "ازم", "بهمون", "برامون", "بهت", "ولی", "اما", "یا", "اگه",

            "داد", "دادم", "دادن", "دادند", "گرفت", "گرفتم", "گرفتند", "پرداخت", "پرداختم",
            "واریز", "دریافت", "کرد", "کردم", "کردند", "کن", "کنید", "خرید", "خریدم",
            "فروخت", "فروختم", "فروش", "جنس", "کالا", "بود", "شد", "شده", "است", "هست",
            "نوشتم", "کشیدم", "صادر", "فرستادم", "فرستاد", "آورد", "آوردم", "رفت", "آمد",
            "اومد", "ثبت", "لطفا", "لطفاً", "می", "نمی",

            "چک", "چکی", "چکها", "ها", "های", "مبلغ", "سررسید", "تاریخ", "شماره", "شمارهٔ",
            "سری", "سریال", "صیاد", "صیادی", "بانک", "فقره", "پیگیری", "حساب", "مشتری",
            "بدهی", "بدهکار", "قرض", "طلب", "نقد", "نقدی", "کارت", "کارتخوان", "پایانه",
            "حواله", "فیش", "رسید", "پول",

            "امروز", "دیروز", "فردا", "پس", "الان", "صبح", "ظهر", "عصر", "شب", "امشب",
            "هفته", "ماه", "سال", "روز", "دیگه", "دیگر", "بعد", "قبل", "پیش", "آینده",
            "گذشته", "آخر", "وسط", "ساعت", "دقیقه"
        };

        foreach (var word in MoneyWords)
            words.Add(word);

        foreach (var word in PersianNumberVocabulary.Units.Keys)
            words.Add(word);

        foreach (var word in OrdinalWords)
            words.Add(word);

        return words;
    }

    [GeneratedRegex(@"[\p{L}]+|[\p{N}][\p{N},.\/\-]*")]
    private static partial Regex TokenRegex();

    private sealed record Token(
        string Text,
        int Start,
        int End,
        bool IsNumber);

    private sealed record NameRun(
        int Start,
        int End,
        double Score);
}
