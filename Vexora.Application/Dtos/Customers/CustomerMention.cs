namespace Vexora.Application.Dtos.Customers;

public sealed record CustomerMention
(
    string RawValue,

    int Start ,

    int Length,

    double Confidence
);
