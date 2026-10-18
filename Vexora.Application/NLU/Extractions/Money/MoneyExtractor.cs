using System.Text.RegularExpressions;
using Vexora.Application.NLU.Abstractions.Extractions.Money;
using Vexora.Application.NLU.Abstractions.Money;
using Vexora.Application.NLU.Models.Entities;
using Vexora.Application.NLU.Normalizations.Parsing.Money;

namespace Vexora.Application.NLU.Extractions;

public sealed partial class MoneyExtractor : IMoneyExtractor
{
    private const double CurrencyConfidence = 0.98;
    private const double MultiplierConfidence = 0.90;
    private const double BareNumberConfidence = 0.75;

    private readonly IMoneyParser _moneyParser;

    public MoneyExtractor(IMoneyParser moneyParser)
    {
        _moneyParser = moneyParser;
    }

    public EntityType EntityType => EntityType.Money;

    public IReadOnlyCollection<EntityResult> Extract(
        string normalizedText)
    {
        if (string.IsNullOrWhiteSpace(normalizedText))
            return [];

        var prepared = MaskNonMoney(
            MoneyTokens.Prepare(normalizedText));

        var spans = FindSpans(
            MoneyTokens.Tokenize(prepared));

        var results = new List<EntityResult>();

        foreach (var span in spans.Where(x => x.HasCurrency || x.HasMultiplier))
        {
            AddResult(
                results,
                normalizedText,
                span,
                span.HasCurrency
                    ? CurrencyConfidence
                    : MultiplierConfidence);
        }

        if (results.Count > 0)
            return results;

        foreach (var span in spans.Where(x => x.IsBareNumber))
        {
            AddResult(
                results,
                normalizedText,
                span,
                BareNumberConfidence);
        }

        return results;
    }

    private void AddResult(
        List<EntityResult> results,
        string originalText,
        MoneySpan span,
        double confidence)
    {
        var rawValue = originalText
            .Substring(span.Start, span.End - span.Start)
            .Trim();

        if (!_moneyParser.TryParse(rawValue, out var money))
            return;

        results.Add(
            new EntityResult(
                EntityType.Money,
                rawValue,
                money,
                span.Start,
                span.End - span.Start,
                confidence));
    }

    private static List<MoneySpan> FindSpans(
        IReadOnlyList<MoneyToken> tokens)
    {
        var spans = new List<MoneySpan>();

        var index = 0;

        while (index < tokens.Count)
        {
            if (!IsNumberLike(tokens[index].Kind))
            {
                index++;
                continue;
            }

            var first = index;
            var last = index;
            var previous = tokens[index].Kind;
            var hasCurrency = false;

            var next = index + 1;

            while (next < tokens.Count)
            {
                var kind = tokens[next].Kind;

                if (kind == MoneyTokenKind.Currency)
                {
                    if (previous != MoneyTokenKind.And)
                    {
                        last = next;
                        hasCurrency = true;
                        next++;
                    }

                    break;
                }

                if (kind == MoneyTokenKind.And)
                {
                    var followedByNumber =
                        next + 1 < tokens.Count &&
                        IsNumberLike(tokens[next + 1].Kind);

                    if (previous == MoneyTokenKind.And || !followedByNumber)
                        break;

                    previous = MoneyTokenKind.And;
                    next++;
                    continue;
                }

                if (!CanFollow(tokens, next, previous))
                    break;

                previous = kind;
                last = next;
                next++;
            }

            spans.Add(new MoneySpan(
                tokens[first].Start,
                tokens[last].End,
                tokens.Skip(first).Take(last - first + 1).ToList(),
                hasCurrency));

            index = next;
        }

        return spans;
    }

    private static bool CanFollow(
        IReadOnlyList<MoneyToken> tokens,
        int position,
        MoneyTokenKind previous)
    {
        switch (tokens[position].Kind)
        {
            case MoneyTokenKind.Number:
            case MoneyTokenKind.Word:
                if (previous == MoneyTokenKind.And)
                    return true;

                return previous == MoneyTokenKind.Multiplier &&
                       position + 1 < tokens.Count &&
                       tokens[position + 1].Kind == MoneyTokenKind.Multiplier;

            case MoneyTokenKind.Multiplier:
                return previous is
                    MoneyTokenKind.Number or
                    MoneyTokenKind.Word or
                    MoneyTokenKind.Half;

            case MoneyTokenKind.Half:
                return previous == MoneyTokenKind.And;

            default:
                return false;
        }
    }

    private static bool IsNumberLike(MoneyTokenKind kind)
        => kind is
            MoneyTokenKind.Number or
            MoneyTokenKind.Word or
            MoneyTokenKind.Multiplier or
            MoneyTokenKind.Half;

    private static string MaskNonMoney(string text)
    {
        foreach (var regex in new[]
                 {
                     FullDateRegex(),
                     ShortDateRegex(),
                     MonthDateRegex(),
                     DurationRegex(),
                     CheckNumberRegex(),
                     LongDigitsRegex(),
                     MobileNumberRegex()
                 })
        {
            text = regex.Replace(
                text,
                match => new string(' ', match.Length));
        }

        return text;
    }

    private sealed record MoneySpan(
        int Start,
        int End,
        IReadOnlyList<MoneyToken> Tokens,
        bool HasCurrency)
    {
        public bool HasMultiplier =>
            Tokens.Any(x => x.Kind == MoneyTokenKind.Multiplier);

        public bool IsBareNumber =>
            !HasCurrency &&
            Tokens.Count == 1 &&
            Tokens[0].Kind == MoneyTokenKind.Number;
    }

    [GeneratedRegex(@"\d{4}[/\-]\d{1,2}[/\-]\d{1,2}")]
    private static partial Regex FullDateRegex();

    [GeneratedRegex(@"(?<![\d,.])\d{1,2}[/\-]\d{1,2}(?![\d,.])")]
    private static partial Regex ShortDateRegex();

    [GeneratedRegex(@"(?<![\d,.])\d{1,2}\s*(?:ام)?\s*(?:فروردین|اردیبهشت|خرداد|تیر|مرداد|شهریور|مهر|آبان|آذر|دی|بهمن|اسفند)(?!\p{L})(?:\s*ماه)?(?:\s+1[34]\d{2}(?!\d))?")]
    private static partial Regex MonthDateRegex();

    [GeneratedRegex(@"(?<![\d,.])\d+(?:\.\d+)?\s*(?:روز|هفته|ماه|سال|ساعت|دقیقه)(?!\p{L})")]
    private static partial Regex DurationRegex();

    [GeneratedRegex(@"(?<!\p{L})(?:شماره|ش|سریال|سری|صیاد|صیادی|پیگیری)\s*[:\-]?\s*\d[\d\-/]*")]
    private static partial Regex CheckNumberRegex();

    [GeneratedRegex(@"(?<![\d,.])\d{12,}(?![\d,.])")]
    private static partial Regex LongDigitsRegex();

    [GeneratedRegex(@"(?<![\d,.])0?9\d{9}(?![\d,.])")]
    private static partial Regex MobileNumberRegex();
}
