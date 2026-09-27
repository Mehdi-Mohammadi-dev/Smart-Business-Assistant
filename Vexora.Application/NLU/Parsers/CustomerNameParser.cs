namespace Vexora.Application.NLU.Parsers;

public sealed class CustomerNameParser : ICustomerNameParser
{
    public bool TryParse(
        string fullName,
        out string name,
        out string family)
    {
        name = string.Empty;
        family = string.Empty;

        if (string.IsNullOrWhiteSpace(fullName))
            return false;

        var parts = fullName
            .Trim()
            .Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 2)
            return false;

        name = parts[0];

        family = string.Join(
            ' ',
            parts.Skip(1));

        return true;
    }
}
