using System.Text.RegularExpressions;
using Vexora.Application.Dtos.Customers;
using Vexora.Application.NLU.Abstractions.Extractions.Customers;

namespace Voxora.Application.NLU.Extraction.Customers;

public sealed partial class CustomerExtractor
    : ICustomerExtractor
{
    public IReadOnlyCollection<CustomerMention> Extract(
        string normalizedText)
    {
        if (string.IsNullOrWhiteSpace(normalizedText))
            return [];

        var results = new List<CustomerMention>();

        foreach (Match match in CustomerPattern().Matches(normalizedText))
        {
            var name = match.Groups["name"]
                .Value
                .Trim();

            if (string.IsNullOrWhiteSpace(name))
                continue;

            results.Add(
                new CustomerMention
                (
                    name,
                    match.Groups["name"].Index,
                    match.Groups["name"].Length,
                    0.85
                ));
        }

        return results;
    }

    [GeneratedRegex(
        @"(?:از|به|برای)\s+(?:(?:آقا|آقای|خانم)\s+)?(?<name>[\p{L}]+(?:\s+[\p{L}]+){0,2})")]
    private static partial Regex CustomerPattern();
}