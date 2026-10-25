using System.Text.RegularExpressions;
using Vexora.Application.NLU.Abstractions.Extractions.Money;
using Vexora.Application.NLU.Abstractions.Money;
using Vexora.Application.NLU.Models.Entities;
using Vexora.Application.NLU.Normalizations.Parsing.Money;

namespace Vexora.Application.NLU.Extractions;

internal sealed record MoneySpan(
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
