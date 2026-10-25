namespace Vexora.Application.Common;

public static class PersonName
{
    public static string Full(string firstName, string lastName)
        => $"{firstName} {lastName}".Trim();
}
