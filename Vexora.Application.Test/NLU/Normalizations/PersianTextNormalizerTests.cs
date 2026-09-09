using FluentAssertions;
using Voxora.Application.NLU.Normalizations;
using Xunit;

namespace Voxora.Application.Tests.NLU.Normalizations;

public sealed class PersianTextNormalizerTests
{
    private readonly PersianTextNormalizer _normalizer = new();

    [Fact]
    public void Should_Normalize_Persian_And_Arabic_Characters()
    {
        var result = _normalizer.Normalize("علي ١٥٠ تومن داد");

        result.Value.Should().Be("علی 150 تومان داد");
    }

    [Fact]
    public void Should_Normalize_Persian_Digits()
    {
        var result = _normalizer.Normalize("علی ۱۵۰ تومن داد");

        result.Value.Should().Be("علی 150 تومان داد");
    }

    [Fact]
    public void Should_Normalize_Arabic_Digits()
    {
        var result = _normalizer.Normalize("علی ١٥٠ تومن داد");

        result.Value.Should().Be("علی 150 تومان داد");
    }

    [Fact]
    public void Should_Normalize_Whitespace()
    {
        var result = _normalizer.Normalize(
            "علی     ۱۵۰      تومن      داد");

        result.Value.Should().Be("علی 150 تومان داد");
    }

    [Fact]
    public void Should_Preserve_Original_Text()
    {
        const string input = "علي ۱۵۰ تومن داد";

        var result = _normalizer.Normalize(input);

        result.Original.Should().Be(input);
    }

    [Fact]
    public void Should_Return_Empty_Value_For_Whitespace()
    {
        var result = _normalizer.Normalize("     ");

        result.Value.Should().BeEmpty();
    }
}