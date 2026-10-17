using Vexora.Application.NLU.Models.Money;

namespace Vexora.Application.NLU.Abstractions.Money;

public interface IMoneyParser
{
    bool TryParse(
        string text,
        out MoneyAmount? money);
}
