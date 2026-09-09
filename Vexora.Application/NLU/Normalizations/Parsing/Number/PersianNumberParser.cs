using System.Globalization;
using System.Text.RegularExpressions;
using Vexora.Application.NLU.Abstractions.Number;
using Vexora.Application.NLU.Models.Number;
using Vexora.Application.NLU.Normalizations.Parsing.Number;

namespace Voxora.Application.NLU.Parsing.Number;

public sealed partial class PersianNumberParser : IPersianNumberParser
{
    public bool TryParse(
        string text,
        out ParsedNumber? result)
    {
        result = null;

        if (string.IsNullOrWhiteSpace(text))
            return false;

        text = Normalize(text);

        // 150
        // 1.5
        // 1,500
        if (TryParseNumeric(text, out var numericValue))
        {
            result = new ParsedNumber
            (
                numericValue
            );

            return true;
        }

        // "دو و نیم"
        if (TryParseFractional(text, out var fractionalValue))
        {
            result = new ParsedNumber
            (
                fractionalValue
            );

            return true;
        }

        // "دو میلیون و پانصد هزار"
        if (TryParseWords(text, out var wordValue))
        {
            result = new ParsedNumber
            (
                wordValue
            );

            return true;
        }

        return false;
    }

    private static string Normalize(string text)
    {
        return MultipleSpacesRegex()
            .Replace(text.Trim(), " ")
            .Replace("‌", " ");
    }

    private static bool TryParseNumeric(
        string text,
        out decimal value)
    {
        value = 0;

        return decimal.TryParse(
            text,
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out value);
    }

    private static bool TryParseFractional(
        string text,
        out decimal value)
    {
        value = 0;

        var tokens = text.Split(
            ' ',
            StringSplitOptions.RemoveEmptyEntries);

        var andIndex = Array.IndexOf(tokens, "و");

        if (andIndex <= 0 || andIndex >= tokens.Length - 1)
            return false;

        var integerPartText = string.Join(
            ' ',
            tokens[..andIndex]);

        var fractionalPartText = string.Join(
            ' ',
            tokens[(andIndex + 1)..]);

        if (!TryParseWords(
                integerPartText,
                out var integerPart))
        {
            return false;
        }

        decimal fraction;

        if (fractionalPartText == "نیم")
        {
            fraction = 0.5m;
        }
        else if (fractionalPartText == "ربع")
        {
            fraction = 0.25m;
        }
        else
        {
            return false;
        }

        value = integerPart + fraction;

        return true;
    }

    private static bool TryParseWords(
        string text,
        out decimal value)
    {
        value = 0;

        var tokens = text.Split(
            ' ',
            StringSplitOptions.RemoveEmptyEntries);

        if (tokens.Length == 0)
            return false;

        decimal total = 0;
        decimal current = 0;

        foreach (var token in tokens)
        {
            if (token == "و")
                continue;

            if (PersianNumberVocabulary.Units
                .TryGetValue(token, out var number))
            {
                current += number;
                continue;
            }

            if (PersianNumberVocabulary.Multipliers
                .TryGetValue(token, out var multiplier))
            {
                if (current == 0)
                    current = 1;

                total += current * multiplier;
                current = 0;

                continue;
            }

            return false;
        }

        value = total + current;

        return value >= 0;
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex MultipleSpacesRegex();
}