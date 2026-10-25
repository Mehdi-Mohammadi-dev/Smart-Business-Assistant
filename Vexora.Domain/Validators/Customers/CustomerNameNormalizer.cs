using System.Text;

namespace Vexora.Domain.Validators.Customers;

/// <summary>
/// Produces a comparison key for Persian customer names without changing the
/// original display name stored on the customer.
/// </summary>
public static class CustomerNameNormalizer
{
    public static string Normalize(string? firstName, string? lastName)
    {
        var fullName = $"{firstName ?? string.Empty} {lastName ?? string.Empty}";
        return Normalize(fullName);
    }

    public static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var normalized = value
            .Normalize(NormalizationForm.FormKC)
            .Replace('\u064a', '\u06cc') // Arabic Yeh -> Persian Yeh
            .Replace('\u0649', '\u06cc') // Alef Maksura -> Persian Yeh
            .Replace('\u0643', '\u06a9'); // Arabic Kaf -> Persian Kaf

        return string.Join(' ', normalized.Split(
            (char[]?)null,
            StringSplitOptions.RemoveEmptyEntries));
    }
}
