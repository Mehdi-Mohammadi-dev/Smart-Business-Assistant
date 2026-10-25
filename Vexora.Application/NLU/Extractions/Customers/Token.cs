namespace Vexora.Application.NLU.Extractions.Customers;

internal sealed record Token(
    string Text,
    int Start,
    int End,
    bool IsNumber);
