using Vexora.Application.NLU.Models.Intents;
using Voxora.Application.NLU.Abstractions.Intents;
using Voxora.Application.NLU.Intents;

namespace Voxora.Application.NLU.Intent;

public sealed class IntentDetector : IIntentDetector
{
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

        return new IntentDetectionResult(
            confidence >= 0.60
                ? best.Intent
                : IntentType.Unknown,
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

        double score = 0;

        foreach (var keyword in matchedKeywords)
        {
            // اگر یک keyword طولانی‌تر همین keyword را
            // پوشش داده، دوباره امتیاز نده.
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

    private static double GetKeywordWeight(IntentType intent, string keyword)
    {
        // 1. اولویت اول: عبارت‌های چندکلمه‌ای
        if (keyword.Contains(' ')) return 4.0;

        // 2. اولویت دوم: تطابق Intent
        double? specificWeight = intent switch
        {
            IntentType.RegisterCheck when keyword == "چک" => 1.8,
            IntentType.RegisterSale when keyword is "فروش" or "جنس" or "کالا" => 1.5,
            IntentType.GetTodaySales when keyword == "فروش" => 1.5,
            IntentType.RegisterPayment when keyword is "پرداخت" or "واریز" => 1.5,
            _ => null
        };

        if (specificWeight.HasValue) return specificWeight.Value;

        // 3. اولویت سوم: استفاده از if-else برای جلوگیری از خطای Unreachable در Switch
        int len = keyword.Length;
        if (len <= 3) return 0.9;
        if (len == 4) return 0.8;
        if (len >= 5 && len <= 7) return 0.6;
        return 0.8; // برای len >= 8 و موارد دیگر
    }



    private static double CalculateConfidence(
    double bestScore,
    double secondScore)
    {
        if (bestScore <= 0)
            return 0;

        if (secondScore <= 0)
            return Math.Min(bestScore, 1.0);

        var difference =
            bestScore - secondScore;

        return Math.Clamp(
            difference / bestScore,
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