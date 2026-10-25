using FluentAssertions;
using Vexora.Application.Ledger.Queries;
using Xunit;

namespace Vexora.Application.Test.Ledger;

public sealed class SearchTextTests
{
    [Fact]
    public void Normalize_UnifiesArabicLettersAndSpaces()
    {
        SearchText.Normalize("  علي   رضايي ").Should().Be("علی رضایی");
        SearchText.Normalize("كريم").Should().Be("کریم");
    }

    [Fact]
    public void Normalize_TurnsHalfSpaceIntoSpace()
    {
        SearchText.Normalize("می\u200cرود").Should().Be("می رود");
    }

    [Fact]
    public void Normalize_ConvertsPersianAndArabicDigits()
    {
        SearchText.Normalize("۱۲۳٤٥").Should().Be("12345");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Normalize_BlankIsNull(string? value)
    {
        SearchText.Normalize(value).Should().BeNull();
    }
}
