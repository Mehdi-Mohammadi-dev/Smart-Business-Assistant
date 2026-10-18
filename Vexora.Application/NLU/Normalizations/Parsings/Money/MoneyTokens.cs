using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Vexora.Application.NLU.Models.Money;
using Vexora.Application.NLU.Normalizations.Parsing.Number;

namespace Vexora.Application.NLU.Normalizations.Parsing.Money;

internal enum MoneyTokenKind
{
    Other,
    Number,
    Word,
    Multiplier,
    Half,
    And,
    Currency
}

internal readonly record struct MoneyToken(
    string Text,
    int Start,
    MoneyTokenKind Kind)
{
    public int End => Start + Text.Length;
}

internal static partial class MoneyTokens
{
    private const string Punctuation = ",.;:!?()[]{}\"'«»،؛؟";

    public static readonly IReadOnlyDictionary<string, decimal> Multipliers =
        new Dictionary<string, decimal>(StringComparer.Ordinal)
        {
            ["هزار"] = 1_000,
            ["میلیون"] = 1_000_000,
            ["میلیونی"] = 1_000_000,
            ["میلیارد"] = 1_000_000_000,
            ["میلیاردی"] = 1_000_000_000
        };

    private static readonly IReadOnlyDictionary<string, MoneyUnit> Currencies =
        new Dictionary<string, MoneyUnit>(StringComparer.Ordinal)
        {
            ["تومان"] = MoneyUnit.Toman,
            ["تومن"] = MoneyUnit.Toman,
            ["تومانی"] = MoneyUnit.Toman,
            ["تومنی"] = MoneyUnit.Toman,
            ["ریال"] = MoneyUnit.Rial,
            ["ریالی"] = MoneyUnit.Rial
        };

    public static MoneyUnit UnitOf(string currencyWord)
        => Currencies[currencyWord];

    public static string Prepare(string text)
    {
        var builder = new StringBuilder(text.Length);

        foreach (var ch in text)
        {
            builder.Append(ch switch
            {
                >= '۰' and <= '۹' => (char)('0' + (ch - '۰')),
                >= '٠' and <= '٩' => (char)('0' + (ch - '٠')),
                '٬' or '،' => ',',
                '٫' => '.',
                '\u200c' => ' ',
                'ي' or 'ى' => 'ی',
                'ك' => 'ک',
                _ => ch
            });
        }

        return builder.ToString();
    }

    public static List<MoneyToken> Tokenize(string preparedText)
    {
        var tokens = new List<MoneyToken>();

        foreach (Match match in WordRegex().Matches(preparedText))
        {
            var start = match.Index;
            var text = match.Value;

            var leading = 0;
            while (leading < text.Length && Punctuation.Contains(text[leading]))
                leading++;

            var length = text.Length - leading;
            while (length > 0 && Punctuation.Contains(text[leading + length - 1]))
                length--;

            if (length <= 0)
                continue;

            AddToken(tokens, text.Substring(leading, length), start + leading);
        }

        return tokens;
    }

    private static void AddToken(
        List<MoneyToken> tokens,
        string text,
        int start)
    {
        var glued = GluedRegex().Match(text);

        if (glued.Success)
        {
            foreach (var group in new[] { "n", "m", "c" })
            {
                var part = glued.Groups[group];

                if (!part.Success)
                    continue;

                var partText = group == "m" && glued.Groups["y"].Success
                    ? part.Value + glued.Groups["y"].Value
                    : part.Value;

                tokens.Add(new MoneyToken(
                    partText,
                    start + part.Index,
                    Classify(partText)));
            }

            return;
        }

        tokens.Add(new MoneyToken(text, start, Classify(text)));
    }

    private static MoneyTokenKind Classify(string text)
    {
        if (NumberRegex().IsMatch(text))
            return MoneyTokenKind.Number;

        if (text == "و")
            return MoneyTokenKind.And;

        if (text is "نیم" or "ربع")
            return MoneyTokenKind.Half;

        if (Multipliers.ContainsKey(text))
            return MoneyTokenKind.Multiplier;

        if (Currencies.ContainsKey(text))
            return MoneyTokenKind.Currency;

        if (PersianNumberVocabulary.Units.ContainsKey(text))
            return MoneyTokenKind.Word;

        return MoneyTokenKind.Other;
    }

    public static bool TryParseNumber(string text, out decimal value)
    {
        value = 0;

        var t = text.TrimEnd(',', '.');

        if (ThousandsWithCommaRegex().IsMatch(t))
            t = t.Replace(",", string.Empty);
        else if (ThousandsWithDotRegex().IsMatch(t))
            t = t.Replace(".", string.Empty);
        else if (DecimalWithCommaRegex().IsMatch(t))
            t = t.Replace(',', '.');
        else if (t.Contains(','))
            return false;

        return decimal.TryParse(
            t,
            NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture,
            out value);
    }

    [GeneratedRegex(@"\S+")]
    private static partial Regex WordRegex();

    [GeneratedRegex(@"^\d[\d,.]*$")]
    private static partial Regex NumberRegex();

    [GeneratedRegex(@"^(?<n>\d[\d,.]*)?(?<m>هزار|میلیون|میلیارد)(?<y>ی)?(?<c>تومان|تومن|تومانی|تومنی|ریال|ریالی)?$")]
    private static partial Regex GluedRegex();

    [GeneratedRegex(@"^\d{1,3}(,\d{3})+(\.\d+)?$")]
    private static partial Regex ThousandsWithCommaRegex();

    [GeneratedRegex(@"^\d{1,3}(\.\d{3}){2,}$")]
    private static partial Regex ThousandsWithDotRegex();

    [GeneratedRegex(@"^\d+,\d{1,2}$")]
    private static partial Regex DecimalWithCommaRegex();
}
