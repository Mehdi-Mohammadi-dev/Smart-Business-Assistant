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

        var tokens = MoneyTokens.Tokenize(
            MoneyTokens.Prepare(text));

        if (tokens.Count == 0)
            return false;

        var unit = MoneyUnit.Toman;

        decimal total = 0;
        decimal current = 0;
        decimal lastMultiplier = 0;
        var hasWholePart = false;

        foreach (var token in tokens)
        {
            switch (token.Kind)
            {
                case MoneyTokenKind.Currency:
                    unit = MoneyTokens.UnitOf(token.Text);
                    break;

                case MoneyTokenKind.And:
                    break;

                case MoneyTokenKind.Number:
                    if (!MoneyTokens.TryParseNumber(token.Text, out var number))
                        return false;

                    current += number;
                    hasWholePart = true;
                    break;

                case MoneyTokenKind.Word:
                    if (!_numberParser.TryParse(token.Text, out var word))
                        return false;

                    current += word!.Value;
                    hasWholePart = true;
                    break;

                case MoneyTokenKind.Multiplier:
                    var multiplier = MoneyTokens.Multipliers[token.Text];

                    if (current == 0)
                        current = 1;

                    total += current * multiplier;
                    current = 0;
                    lastMultiplier = multiplier;
                    hasWholePart = true;
                    break;

                case MoneyTokenKind.Half:
                    var fraction = token.Text == "نیم" ? 0.5m : 0.25m;

                    if (current > 0)
                    {
                        current += fraction;
                    }
                    else if (lastMultiplier > 0)
                    {
                        total += lastMultiplier * fraction;
                        lastMultiplier = 0;
                    }
                    else
                    {
                        current = fraction;
                    }

                    break;

                default:
                    return false;
            }
        }

        if (!hasWholePart)
            return false;

        if (lastMultiplier >= 1_000_000 && current > 0 && current < 1_000)
            current *= lastMultiplier / 1_000;

        total += current;

        if (total <= 0)
            return false;

        result = new MoneyAmount(total, unit);

        return true;
    }
}
