using FluentAssertions;
using Vexora.Application.Common;
using Xunit;

namespace Vexora.Application.Test.Platform;

public sealed class DigitNormalizerTests
{
    [Theory]
    [InlineData("۰۹۱۲۳۴۵۶۷۸۹", "09123456789")]
    [InlineData("٠٩١٢٣٤٥٦٧٨٩", "09123456789")]
    [InlineData(" ۱۲۳ ۴۵۶ ", "123456")]
    public void Normalize_converts_persian_and_arabic_digits_and_removes_whitespace(string input, string expected)
    {
        DigitNormalizer.Normalize(input).Should().Be(expected);
    }

    [Fact]
    public void Phone_normalizer_uses_digit_normalizer()
    {
        PhoneNumberNormalizer.Normalize(" ۰۹۱۲ ۳۴۵۶۷۸۹ ").Should().Be("09123456789");
    }
}
