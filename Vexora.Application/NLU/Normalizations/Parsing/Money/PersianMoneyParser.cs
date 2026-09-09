using Vexora.Application.NLU.Abstractions.Money;
using Vexora.Application.NLU.Abstractions.Number;
using Vexora.Application.NLU.Models.Money;

namespace Vexora.Application.NLU.Normalizations.Parsing.Money;

public sealed class MoneyParser : IMoneyParser
{
    private readonly IPersianNumberParser _numberParser;

    public MoneyParser(IPersianNumberParser numberParser)
    {
        _numberParser = numberParser;
    }

    public bool TryParse(
        string text,
        out MoneyAmount? result)
    {
        result = null;

        if (string.IsNullOrWhiteSpace(text))
            return false;

        var normalized = Normalize(text);

        var tokens = normalized.Split(
            ' ',
            StringSplitOptions.RemoveEmptyEntries);

        var unit = MoneyUnit.Toman;

        var numberTokens = new List<string>();

        foreach (var token in tokens)
        {
            switch (token)
            {
                case "تومان":
                    unit = MoneyUnit.Toman;
                    break;

                case "ریال":
                    unit = MoneyUnit.Rial;
                    break;

                case "تومن":
                    unit = MoneyUnit.Toman;
                    break;

                default:
                    numberTokens.Add(token);
                    break;
            }
        }

        if (numberTokens.Count == 0)
            return false;

        var numberText = string.Join(' ', numberTokens);

        if (!_numberParser.TryParse(
                numberText,
                out var parsedNumber))
        {
            return false;
        }

        result = new MoneyAmount
        (
            parsedNumber!.Value,
            unit
        );

        return true;
    }

    private static string Normalize(string text)
    {
        return text
            .Trim()
            .Replace("تومن", "تومان")
            .Replace(
                "٬",
                ",");
    }
}