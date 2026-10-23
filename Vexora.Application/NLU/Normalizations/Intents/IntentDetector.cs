using Vexora.Application.NLU.Abstractions.Intents;
using Vexora.Application.NLU.Models.Intents;

namespace Vexora.Application.NLU.Normalizations.Intents;

public sealed class IntentDetector : IIntentDetector
{
    private const double MinimumScore = 1.0;
    private const double MinimumConfidence = 0.60;
    private const double CheckWordScore = 3.5;

    public IntentDetectionResult Detect(string normalizedText)
    {
        if (string.IsNullOrWhiteSpace(normalizedText))
            return Unknown();

        var candidates = new List<IntentCandidate>();

        var tokens = PhraseMatcher.Tokenize(normalizedText);

        foreach (var (intent, keywords) in IntentKeywords.Keywords)
        {
            var score = CalculateScore(
                tokens,
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

        if (candidates.Any(x =>
                x.Intent == IntentType.RegisterCheck &&
                x.Score >= CheckWordScore))
        {
            candidates.RemoveAll(x =>
                x.Intent == IntentType.RegisterPayment);
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
        IReadOnlyList<string> tokens,
        IntentType intent,
        IEnumerable<string> keywords)
    {
        var matchedKeywords = keywords
            .Where(keyword =>
                PhraseMatcher.ContainsPhrase(
                    tokens,
                    keyword))
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
            IntentType.RegisterPayment
                when keyword is "پرداخت" or "واریز"
                => 2.5,

            IntentType.RegisterPayment
                when keyword is "گرفتم" or "گرفت" or "دریافت"
                => 1.5,

            IntentType.RegisterPayment
                when keyword is "داد" or "دادم"
                => 1.5,

            IntentType.RegisterCheck
                when keyword is "چک" or "چکی" or "چکها"
                => 3.5,

            IntentType.RegisterSale
                when keyword == "فروش"
                => 2.5,

            IntentType.RegisterSale
                when keyword is "جنس" or "کالا"
                => 1.5,

            IntentType.GetTodaySales
                when keyword == "فروش"
                => 1.5,

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

        if (secondScore <= 0)
        {
            return Math.Clamp(
                bestScore / 2.0,
                0,
                1);
        }

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
