using Vexora.Application.NLU.Models.Number;

namespace Vexora.Application.NLU.Abstractions.Number;

public interface IPersianNumberParser
{
    bool TryParse(string text, out ParsedNumber? result);
}