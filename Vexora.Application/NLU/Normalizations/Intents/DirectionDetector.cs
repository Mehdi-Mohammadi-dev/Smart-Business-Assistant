using Vexora.Application.NLU.Models.Intents;

namespace Vexora.Application.NLU.Normalizations.Intents;    

public static class DirectionDetector
{
    private static readonly string[] GivenPhrases =
    [
        "دادم",
        "پرداخت کردم",
        "پرداختم",
        "واریز کردم",
        "بهش دادم",
        "پس دادم",
        "فرستادم",
        "نوشتم",
        "کشیدم",
        "صادر کردم",
        "گرفت",
        "دریافت کرد",
        "کارت به کارت کردم"
    ];

    private static readonly string[] ReceivedPhrases =
    [
        "داد",
        "گرفتم",
        "دریافت کردم",
        "پرداخت کرد",
        "واریز کرد",
        "بهم داد",
        "به من داد",
        "برام فرستاد",
        "برام واریز کرد",
        "کارت به کارت کرد"
    ];

    public static DirectionHint Detect(string normalizedText)
    {
        if (string.IsNullOrWhiteSpace(normalizedText))
            return DirectionHint.Unspecified;

        var tokens = PhraseMatcher.Tokenize(normalizedText);

        var given = GivenPhrases.Any(phrase =>
            PhraseMatcher.ContainsPhrase(tokens, phrase));

        var received = ReceivedPhrases.Any(phrase =>
            PhraseMatcher.ContainsPhrase(tokens, phrase));

        if (given == received)
            return DirectionHint.Unspecified;

        return given
            ? DirectionHint.Given
            : DirectionHint.Received;
    }
}
