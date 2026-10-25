using FluentAssertions;
using Vexora.Domain.Entities.Customers;
using Xunit;

namespace Vexora.Application.Test.Zero;

public sealed class CustomerNameNormalizerTests
{
    [Theory]
    [InlineData("علي رضايي", "علی رضایی")]
    [InlineData("كريمي", "کریمی")]
    [InlineData("  علی    رضایی  ", "علی رضایی")]
    [InlineData("علی‌رضایی", "علی رضایی")]
    [InlineData("علـــی", "علی")]
    [InlineData("عَلی", "علی")]
    [InlineData("Ali Rezaei", "ali rezaei")]
    [InlineData("حاج ۱۲", "حاج 12")]
    [InlineData("", "")]
    [InlineData("   ", "")]
    public void Should_Make_Different_Spellings_Equal(
        string input,
        string expected)
    {
        CustomerNameNormalizer.Normalize(input).Should().Be(expected);
    }

    [Fact]
    public void Should_Join_First_And_Last_Name()
    {
        CustomerNameNormalizer.Normalize("علي", "رضايي").Should().Be("علی رضایی");
        CustomerNameNormalizer.Normalize("علی", "").Should().Be("علی");
        CustomerNameNormalizer.Normalize("علی", null).Should().Be("علی");
    }

    [Fact]
    public void Should_Not_Make_Different_People_Equal()
    {
        CustomerNameNormalizer.Normalize("علی رضایی")
            .Should().NotBe(CustomerNameNormalizer.Normalize("علی رضایی بنا"));
    }
}
