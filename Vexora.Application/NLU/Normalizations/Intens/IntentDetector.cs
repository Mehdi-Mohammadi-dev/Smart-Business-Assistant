using Vexora.Application.NLU.Abstractions.Intents;
using Vexora.Application.NLU.Intents;
using Vexora.Application.NLU.Models.Intents;

namespace Vexora.Application.NLU.Intent;

public sealed class IntentDetector : IIntentDetector
{
    private const double MinimumScore = 1.0;
    private const double MinimumConfidence = 0.60;

    public IntentDetectionResult Detect(string normalizedText)
    {
        if (string.IsNullOrWhiteSpace(normalizedText))
            return Unknown();

        var candidates = new List<IntentCandidate>();

        foreach (var (intent, keywords) in IntentKeywords.Keywords)
        {
            var score = CalculateScore(
                normalizedText,
                intent,
                keywords);

            if (score > 0)
            {
                candidates.Add(
                    new IntentCandidate(
                        intent,
                        score));
            }
        }

        if (candidates.Count == 0)
            return Unknown();

        var ordered = candidates
            .OrderByDescending(x => x.Score)
            .ToList();

        var best = ordered[0];

        var secondScore =
            ordered.Count > 1
                ? ordered[1].Score
                : 0;

        var confidence = CalculateConfidence(
            best.Score,
            secondScore);

        var detectedIntent =
            best.Score >= MinimumScore &&
            confidence >= MinimumConfidence
                ? best.Intent
                : IntentType.Unknown;

        return new IntentDetectionResult(
            detectedIntent,
            confidence,
            ordered);
    }

    private static double CalculateScore(
        string text,
        IntentType intent,
        IEnumerable<string> keywords)
    {
        var matchedKeywords = keywords
            .Where(keyword =>
                text.Contains(
                    keyword,
                    StringComparison.Ordinal))
            .OrderByDescending(keyword => keyword.Length)
            .ToList();

        if (matchedKeywords.Count == 0)
            return 0;

        double score = 0;

        foreach (var keyword in matchedKeywords)
        {
            var isCoveredByLongerKeyword =
                matchedKeywords.Any(longerKeyword =>
                    longerKeyword.Length > keyword.Length &&
                    longerKeyword.Contains(
                        keyword,
                        StringComparison.Ordinal));

            if (isCoveredByLongerKeyword)
                continue;

            score += GetKeywordWeight(
                intent,
                keyword);
        }

        return score;
    }

    private static double GetKeywordWeight(
        IntentType intent,
        string keyword)
    {
        // عبارت‌های چندکلمه‌ای، شواهد قوی‌تری هستند.
        var wordCount =
            keyword.Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries)
            .Length;

        if (wordCount >= 2)
        {
            return 3.0 + (wordCount * 0.5);
        }

        return intent switch
        {
            // پرداخت
            IntentType.RegisterPayment
                when keyword is "پرداخت" or "واریز"
                => 2.5,

            IntentType.RegisterPayment
                when keyword is "گرفتم" or "گرفت" or "دریافت"
                => 1.5,

            // چک
            IntentType.RegisterCheck
                when keyword == "چک"
                => 3.5,

            // فروش
            IntentType.RegisterSale
                when keyword == "فروش"
                => 2.5,

            IntentType.RegisterSale
                when keyword is "جنس" or "کالا"
                => 1.5,

            // گزارش فروش
            IntentType.GetTodaySales
                when keyword == "فروش"
                => 1.5,

            // کلمات عمومی
            _ => GetGenericKeywordWeight(keyword)
        };
    }

    private static double GetGenericKeywordWeight(
        string keyword)
    {
        return keyword.Length switch
        {
            <= 3 => 0.9,
            4 => 0.8,
            5 or 6 or 7 => 0.6,
            _ => 0.8
        };
    }

    private static double CalculateConfidence(
        double bestScore,
        double secondScore)
    {
        if (bestScore <= 0)
            return 0;

        // فقط یک Intent کاندید وجود دارد.
        // در این حالت نباید صرفاً به خاطر پایین بودن score
        // آن را Unknown کنیم.
        if (secondScore <= 0)
        {
            return Math.Clamp(
                bestScore / 2.0,
                0,
                1);
        }

        // وقتی چند Intent رقیب داریم،
        // نسبت شواهد Intent برتر را محاسبه می‌کنیم.
        return Math.Clamp(
            bestScore / (bestScore + secondScore),
            0,
            1);
    }

    private static IntentDetectionResult Unknown()
    {
        return new IntentDetectionResult(
            IntentType.Unknown,
            0,
            []);
    }
}