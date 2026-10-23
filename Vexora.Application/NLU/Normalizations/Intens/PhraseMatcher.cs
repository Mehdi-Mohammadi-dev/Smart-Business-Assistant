using System.Text.RegularExpressions;

namespace Vexora.Application.NLU.Intent;

internal static partial class PhraseMatcher
{
    public static IReadOnlyList<string> Tokenize(string text)
    {
        return TokenRegex()
            .Matches(text)
            .Select(match => NormalizeToken(match.Value))
            .ToArray();
    }

    private static string NormalizeToken(string token)
        => token == "های" ? "ها" : token;

    public static bool ContainsPhrase(
        IReadOnlyList<string> textTokens,
        string phrase)
    {
        var phraseTokens = phrase
            .Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries)
            .Select(NormalizeToken)
            .ToArray();

        if (phraseTokens.Length == 0 || phraseTokens.Length > textTokens.Count)
            return false;

        for (var start = 0; start <= textTokens.Count - phraseTokens.Length; start++)
        {
            var matched = true;

            for (var offset = 0; offset < phraseTokens.Length; offset++)
            {
                if (!string.Equals(
                        textTokens[start + offset],
                        phraseTokens[offset],
                        StringComparison.Ordinal))
                {
                    matched = false;
                    break;
                }
            }

            if (matched)
                return true;
        }

        return false;
    }

    [GeneratedRegex(@"[\p{L}\p{N}]+")]
    private static partial Regex TokenRegex();
}
