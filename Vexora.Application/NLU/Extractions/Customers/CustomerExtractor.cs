using System.Text.RegularExpressions;
using Vexora.Application.Dtos.Customers;
using Vexora.Application.NLU.Abstractions.Extractions.Customers;
using Vexora.Application.NLU.Models.Entities;

namespace Vexora.Application.NLU.Extractions.Customers;

public sealed partial class CustomerExtractor : ICustomerExtractor
{
    private static readonly HashSet<string> IgnoredNames =
[
    "امروز",
    "دیروز",
    "فردا",
    "الان",
    "صبح",
    "شب",
    "امروز صبح",
    "امشب"
];

    public EntityType EntityType => EntityType.Customer;

    public IReadOnlyCollection<EntityResult> Extract(
    string normalizedText)
    {
        if (string.IsNullOrWhiteSpace(normalizedText))
            return [];

        var results = new List<EntityResult>();

        ExtractWithPreposition(
            normalizedText,
            results);

        if (results.Count == 0)
        {
            ExtractBeforeMoney(
                normalizedText,
                results);
        }

        return results;
    }

    private static void ExtractWithPreposition(
    string text,
    List<EntityResult> results)
    {
        foreach (Match match in CustomerRegex().Matches(text))
        {
            var name = match.Groups["name"]
                .Value
                .Trim();

            if (string.IsNullOrWhiteSpace(name))
                continue;

            results.Add(
                new EntityResult(
                    EntityType.Customer,
                    name,
                    new CustomerMention(
                        name,
                        match.Groups["name"].Index,
                        match.Groups["name"].Length,
                        0.85),
                    match.Groups["name"].Index,
                    match.Groups["name"].Length,
                    0.85));
        }
    }

    private static void ExtractBeforeMoney(
    string text,
    List<EntityResult> results)
    {
        var match = CustomerBeforeMoneyRegex()
            .Match(text);

        if (!match.Success)
            return;

        var name = match.Groups["name"]
            .Value
            .Trim();


        if (IgnoredNames.Contains(name))
            return;


        results.Add(
            new EntityResult(
                EntityType.Customer,
                name,
                new CustomerMention(
                    name,
                    match.Groups["name"].Index,
                    match.Groups["name"].Length,
                    0.75),
                match.Groups["name"].Index,
                match.Groups["name"].Length,
                0.75));
    }

    [GeneratedRegex(
        @"^(?<name>[\p{L}]{2,}(?:\s+[\p{L}]{2,}){0,2})\s+(?=[0-9۰-۹]+.*?(تومان|تومن|ریال)|(یک|دو|سه|چهار|پنج|شش|هفت|هشت|نه|ده).*?(تومان|تومن|ریال))",
        RegexOptions.Compiled)]
    private static partial Regex CustomerBeforeMoneyRegex();


    [GeneratedRegex(
     @"(?:از|به|برای)\s+(?<name>[\p{L}]{2,}(?:\s+(?!یک|دو|سه|چهار|پنج|شش|هفت|هشت|نه|ده|چک|فقره)[\p{L}]{2,}){0,2})",
     RegexOptions.Compiled)]
    private static partial Regex CustomerRegex();
}