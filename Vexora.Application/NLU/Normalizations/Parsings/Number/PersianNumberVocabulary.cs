namespace Vexora.Application.NLU.Normalizations.Parsing.Number;

internal static class PersianNumberVocabulary
{
    public static readonly IReadOnlyDictionary<string, decimal> Units =
        new Dictionary<string, decimal>(StringComparer.Ordinal)
        {
            ["صفر"] = 0,
            ["یک"] = 1,
            ["دو"] = 2,
            ["سه"] = 3,
            ["چهار"] = 4,
            ["پنج"] = 5,
            ["شش"] = 6,
            ["هفت"] = 7,
            ["هشت"] = 8,
            ["نه"] = 9,

            ["ده"] = 10,
            ["یازده"] = 11,
            ["دوازده"] = 12,
            ["سیزده"] = 13,
            ["چهارده"] = 14,
            ["پانزده"] = 15,
            ["شانزده"] = 16,
            ["هفده"] = 17,
            ["هجده"] = 18,
            ["نوزده"] = 19,

            ["بیست"] = 20,
            ["سی"] = 30,
            ["چهل"] = 40,
            ["پنجاه"] = 50,
            ["شصت"] = 60,
            ["هفتاد"] = 70,
            ["هشتاد"] = 80,
            ["نود"] = 90,

            ["صد"] = 100,
            ["دویست"] = 200,
            ["سیصد"] = 300,
            ["چهارصد"] = 400,
            ["پانصد"] = 500,
            ["ششصد"] = 600,
            ["هفتصد"] = 700,
            ["هشتصد"] = 800,
            ["نهصد"] = 900
        };

    public static readonly IReadOnlyDictionary<string, decimal> Multipliers =
        new Dictionary<string, decimal>(StringComparer.Ordinal)
        {
            ["هزار"] = 1_000,
            ["میلیون"] = 1_000_000,
            ["میلیارد"] = 1_000_000_000
        };
}
