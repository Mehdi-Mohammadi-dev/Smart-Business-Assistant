namespace Vexora.Application.NLU.Models.Money;

public sealed record MoneyAmount
(
    decimal Amount,
    MoneyUnit Unit)
{
    public decimal AmountInToman =>
        Unit == MoneyUnit.Rial
            ? Amount / 10
            : Amount;
}
