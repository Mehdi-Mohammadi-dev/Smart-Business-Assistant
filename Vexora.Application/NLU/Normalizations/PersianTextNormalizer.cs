using System.Text;
using System.Text.RegularExpressions;
using Vexora.Application.NLU.Abstractions;
using Vexora.Application.NLU.Models;

namespace Vexora.Application.NLU.Normalizations;

public sealed partial class PersianTextNormalizer : ITextNormalizer
{
    public NormalizedText Normalize(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        if (string.IsNullOrWhiteSpace(text))
        {
            return new NormalizedText
            (
                 text,
                string.Empty
            );
        }

        var normalized = text.Normalize(NormalizationForm.FormC);

        normalized = NormalizeCharacters(normalized);
        normalized = NormalizeDigits(normalized);
        normalized = NormalizeWhitespace(normalized);
        normalized = NormalizePunctuation(normalized);
        normalized = NormalizeBusinessTerms(normalized);

        return new NormalizedText
        (
             text,
            normalized.Trim()
        );
    }

    private static string NormalizeCharacters(string text)
    {
        return text
            .Replace('ي', 'ی')
            .Replace('ى', 'ی')
            .Replace('ك', 'ک')
            .Replace('ؤ', 'و')
            .Replace('إ', 'ا')
            .Replace('أ', 'ا')
            .Replace('ٱ', 'ا')
            .Replace('ۀ', 'ه')
            .Replace('ة', 'ه');
    }

    private static string NormalizeDigits(string text)
    {
        var chars = text.ToCharArray();

        for (var i = 0; i < chars.Length; i++)
        {
            chars[i] = chars[i] switch
            {
                '۰' => '0',
                '۱' => '1',
                '۲' => '2',
                '۳' => '3',
                '۴' => '4',
                '۵' => '5',
                '۶' => '6',
                '۷' => '7',
                '۸' => '8',
                '۹' => '9',

                '٠' => '0',
                '١' => '1',
                '٢' => '2',
                '٣' => '3',
                '٤' => '4',
                '٥' => '5',
                '٦' => '6',
                '٧' => '7',
                '٨' => '8',
                '٩' => '9',

                _ => chars[i]
            };
        }

        return new string(chars);
    }

    private static string NormalizeWhitespace(string text)
    {
        return WhitespaceRegex()
            .Replace(text, " ")
            .Trim();
    }

    private static string NormalizePunctuation(string text)
    {
        return text
            .Replace('،', ',')
            .Replace('؛', ';')
            .Replace('؟', '?');
    }

    private static string NormalizeBusinessTerms(string text)
    {
        return BusinessTermRegex()
            .Replace(text, "تومان");
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex(@"\bتومن\b", RegexOptions.IgnoreCase)]
    private static partial Regex BusinessTermRegex();
}
