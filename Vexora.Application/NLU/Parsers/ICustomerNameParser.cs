namespace Vexora.Application.NLU.Parsers;

public interface ICustomerNameParser
{
    bool TryParse(
        string fullName,
        out string name,
        out string family);
}
