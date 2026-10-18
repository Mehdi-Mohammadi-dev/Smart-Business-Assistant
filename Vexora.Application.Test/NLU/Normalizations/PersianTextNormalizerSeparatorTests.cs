using FluentAssertions;
using Vexora.Application.NLU.Normalizations;
using Xunit;

namespace Vexora.Application.Test.NLU.Normalizations;

public sealed class PersianTextNormalizerSeparatorTests
{
    private readonly PersianTextNormalizer _normalizer = new();

    [Theory]
    [InlineData("علی ۵۰۰٬۰۰۰ تومان داد", "علی 500,000 تومان داد")]
    [InlineData("علی ۵۰۰،۰۰۰ تومان داد", "علی 500,000 تومان داد")]
    [InlineData("۲٫۵ میلیون", "2.5 میلیون")]
    public void Should_Convert_Persian_Separators(
        string input,
        string expected)
    {
        _normalizer.Normalize(input).Value.Should().Be(expected);
    }
}
