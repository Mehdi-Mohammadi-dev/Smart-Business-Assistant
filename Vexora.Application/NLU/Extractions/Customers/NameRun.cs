namespace Vexora.Application.NLU.Extractions.Customers;

internal sealed record NameRun(
    int Start,
    int End,
    double Score);
