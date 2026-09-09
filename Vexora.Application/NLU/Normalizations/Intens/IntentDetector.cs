using Vexora.Application.NLU.Models.Intents;
using Voxora.Application.NLU.Abstractions.Intents;
using Voxora.Application.NLU.Intents;

namespace Voxora.Application.NLU.Intent;

public sealed class IntentDetector : IIntentDetector
{
    public IntentDetectionResult Detect(string normalizedText)
    {
        if (string.IsNullOrWhiteSpace(normalizedText))
        {
            return Unknown();
        }

        var candidates = new List<IntentCandidate>();

        foreach (var (intent, keywords) in IntentKeywords.Keywords)
        {
            var score = CalculateScore(normalizedText, keywords);

            if (score > 0)
            {
                candidates.Add(
                    new IntentCandidate
                    (
                        intent,
                        score
                    ));
            }
        }

        if (candidates.Count == 0)
        {
            return Unknown();
        }

        var ordered = candidates
            .OrderByDescending(x => x.Score)
            .ToList();

        var best = ordered[0];

        var confidence = CalculateConfidence(
            best.Score,
            ordered.Count > 1 ? ordered[1].Score : 0);

        return new IntentDetectionResult
        (
            confidence >= 0.60
                ? best.Intent
                : IntentType.Unknown,
            confidence,
            ordered
        );
    }

    private static double CalculateScore(
        string text,
        IEnumerable<string> keywords)
    {
        double score = 0;

        foreach (var keyword in keywords)
        {
            if (text.Contains(
                    keyword,
                    StringComparison.Ordinal))
            {
                score += GetKeywordWeight(keyword);
            }
        }

        return score;
    }

    private static double GetKeywordWeight(string keyword)
    {
        return keyword.Length switch
        {
            >= 12 => 1.0,
            >= 8 => 0.8,
            >= 5 => 0.6,
            _ => 0.4
        };
    }

    private static double CalculateConfidence(
        double bestScore,
        double secondScore)
    {
        if (bestScore <= 0)
            return 0;

        if (secondScore <= 0)
            return Math.Min(bestScore / 1.5, 1.0);

        var difference = bestScore - secondScore;

        return Math.Clamp(
            difference / bestScore,
            0,
            1);
    }

    private static IntentDetectionResult Unknown()
    {
        return new IntentDetectionResult
        (
            IntentType.Unknown,
            0,
            []
        );
    }
}