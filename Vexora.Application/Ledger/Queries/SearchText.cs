using System.Text;

namespace Vexora.Application.Ledger.Queries;

public static class SearchText
{
    public static string? Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var builder = new StringBuilder(value.Length);

        foreach (var character in value)
        {
            builder.Append(character switch
            {
                'ي' or 'ى' => 'ی',
                'ك' => 'ک',
                '\u200c' => ' ',
                >= '۰' and <= '۹' => (char)('0' + (character - '۰')),
                >= '٠' and <= '٩' => (char)('0' + (character - '٠')),
                _ => character
            });
        }

        var parts = builder.ToString()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return parts.Length == 0 ? null : string.Join(' ', parts);
    }
}
