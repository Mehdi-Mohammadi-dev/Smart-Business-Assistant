using System.Text.RegularExpressions;
using Vexora.Application.NLU.Abstractions.Extractions.Money;
using Vexora.Application.NLU.Abstractions.Money;
using Vexora.Application.NLU.Models.Entities;

namespace Vexora.Application.NLU.Extractions;

public sealed partial class MoneyExtractor : IMoneyExtractor
{
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

        var results = new List<EntityResult>();

        ExtractExplicitMoney(
            normalizedText,
            results);

        if (results.Count == 0)
        {
            ExtractStandaloneNumber(
                normalizedText,
                results);
        }

        return results;
    }

    private void ExtractExplicitMoney(
        string text,
        List<EntityResult> results)
    {
        foreach (Match match in ExplicitMoneyRegex().Matches(text))
        {
            var rawValue = match.Value.Trim();

            if (!_moneyParser.TryParse(
                    rawValue,
                    out var money))
            {
                continue;
            }

            results.Add(
                new EntityResult(
                    EntityType.Money,
                    rawValue,
                    money,
                    match.Index,
                    match.Length,
                    0.98));
        }
    }

    private void ExtractStandaloneNumber(
        string text,
        List<EntityResult> results)
    {
        foreach (Match match in NumberRegex().Matches(text))
        {
            var rawValue = match.Value;

            if (!_moneyParser.TryParse(
                    rawValue,
                    out var money))
            {
                continue;
            }

            results.Add(
                new EntityResult(
                    EntityType.Money,
                    rawValue,
                    money,
                    match.Index,
                    match.Length,
                    0.75));
        }
    }

    [GeneratedRegex(
        @"(?:(?:[0-9۰-۹][0-9۰-۹,]*)|(?:یک|دو|سه|چهار|پنج|شش|شیش|هفت|هشت|نه|ده|یازده|دوازده|سیزده|چهارده|پانزده|شانزده|هفده|هجده|نوزده|بیست|سی|چهل|پنجاه|شصت|هفتاد|هشتاد|نود|صد|دویست|سیصد|چهارصد|پانصد|ششصد|هفتصد|هشتصد|نهصد)(?:\s+(?:و\s+)?(?:یک|دو|سه|چهار|پنج|شش|شیش|هفت|هشت|نه|ده|بیست|سی|چهل|پنجاه|صد|هزار|میلیون|میلیارد|نیم))*)(?:\s+(?:هزار|میلیون|میلیونی|میلیارد|میلیاردی))?(?:\s+(?:تومان|تومن|تومنی|تومانی|ریال|ریالی))",
        RegexOptions.Compiled)]
    private static partial Regex ExplicitMoneyRegex();

    [GeneratedRegex(
        @"(?<!\d)[0-9۰-۹][0-9۰-۹,]*(?!\d)")]
    private static partial Regex NumberRegex();
}