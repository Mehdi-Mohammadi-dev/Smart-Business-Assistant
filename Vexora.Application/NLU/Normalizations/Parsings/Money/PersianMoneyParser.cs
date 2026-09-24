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

        decimal total = 0;
        decimal currentNumber = 0;

        foreach (var token in tokens)
        {
            switch (token)
            {
                case "تومان":
                case "تومن":
                    unit = MoneyUnit.Toman;
                    break;

                case "ریال":
                    unit = MoneyUnit.Rial;
                    break;

                case "و":
                    break;

                case "هزار":
                    total += currentNumber * 1_000;
                    currentNumber = 0;
                    break;

                case "میلیون":
                case "میلیونی":
                    total += currentNumber * 1_000_000;
                    currentNumber = 0;
                    break;

                case "میلیارد":
                case "میلیاردی":
                    total += currentNumber * 1_000_000_000;
                    currentNumber = 0;
                    break;

                default:

                    if (!_numberParser.TryParse(
                            token,
                            out var parsedNumber))
                    {
                        return false;
                    }

                    currentNumber += parsedNumber!.Value;
                    break;
            }
        }

        total += currentNumber;

        if (total <= 0)
            return false;

        result = new MoneyAmount(
            total,
            unit);

        return true;
    }

    private static string Normalize(string text)
    {
        return text
            .Trim()
            .Replace("تومن", "تومان")
            .Replace("٬", ",")
            .Replace('۰', '0')
            .Replace('۱', '1')
            .Replace('۲', '2')
            .Replace('۳', '3')
            .Replace('۴', '4')
            .Replace('۵', '5')
            .Replace('۶', '6')
            .Replace('۷', '7')
            .Replace('۸', '8')
            .Replace('۹', '9')
            .Replace('٠', '0')
            .Replace('١', '1')
            .Replace('٢', '2')
            .Replace('٣', '3')
            .Replace('٤', '4')
            .Replace('٥', '5')
            .Replace('٦', '6')
            .Replace('٧', '7')
            .Replace('٨', '8')
            .Replace('٩', '9');
    }
}