using System.Text;

namespace Vexora.Domain.Entities.Customers;

public static class CustomerNameNormalizer
{
    public static string Normalize(string? firstName, string? lastName)
        => Normalize($"{firstName} {lastName}");

    public static string Normalize(string? fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return string.Empty;

        var builder = new StringBuilder(fullName.Length);

        foreach (var ch in fullName)
        {
            switch (ch)
            {
                case 'ي':
                case 'ى':
                    builder.Append('ی');
                    break;

                case 'ك':
                    builder.Append('ک');
                    break;

                case >= '۰' and <= '۹':
                    builder.Append((char)('0' + (ch - '۰')));
                    break;

                case >= '٠' and <= '٩':
                    builder.Append((char)('0' + (ch - '٠')));
                    break;

                case '\u200c':
                case '\u00a0':
                case '\u200f':
                case '\u200e':
                    builder.Append(' ');
                    break;

                case 'ـ':
                    break;

                case >= '\u064b' and <= '\u065f':
                case '\u0670':
                    break;

                default:
                    builder.Append(char.ToLowerInvariant(ch));
                    break;
            }
        }

        return string.Join(
            ' ',
            builder
                .ToString()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}
