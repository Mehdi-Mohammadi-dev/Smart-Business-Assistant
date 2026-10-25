using System.Text;

namespace Vexora.Application.Common;

public static class DigitNormalizer
{
    public static string Normalize(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return value ?? string.Empty;

        var builder = new StringBuilder(value.Length);
        foreach (var ch in value)
        {
            if (ch is >= '\u06F0' and <= '\u06F9')
                builder.Append((char)('0' + ch - '\u06F0'));
            else if (ch is >= '\u0660' and <= '\u0669')
                builder.Append((char)('0' + ch - '\u0660'));
            else if (!char.IsWhiteSpace(ch))
                builder.Append(ch);
        }
        return builder.ToString();
    }
}

public static class PhoneNumberNormalizer
{
    public static string Normalize(string? phoneNumber) => DigitNormalizer.Normalize(phoneNumber);
}
