using FluentAssertions;
using Vexora.Application.Dtos.Customers;
using Vexora.Application.NLU.Models.Entities;
using Voxora.Application.NLU.Extractions.Customers;
using Xunit;

namespace Vexora.Application.Test.NLU.Extractions.Customer;

public sealed class CustomerExtractorTests
{
    private readonly CustomerExtractor _extractor;

    public CustomerExtractorTests()
    {
        _extractor = new CustomerExtractor();
    }

    [Fact]
    public void Should_Extract_Customer_Name()
    {
        var result = _extractor.Extract(
            "از مهدی 150 تومان گرفتم");

        result.Should().ContainSingle();

        var entity = result.Single();

        entity.Type.Should().Be(EntityType.Customer);
        entity.RawValue.Should().Be("مهدی");

        var customer = (CustomerMention)entity.Value!;

        customer.RawValue.Should().Be("مهدی");
    }

    [Fact]
    public void Should_Extract_Full_Customer_Name()
    {
        var result = _extractor.Extract(
            "به علی محمدی 10 میلیون تومان جنس دادم");

        result.Should().ContainSingle();

        var entity = result.Single();

        entity.Type.Should().Be(EntityType.Customer);
        entity.RawValue.Should().Be("علی محمدی");

        var customer = (CustomerMention)entity.Value!;

        customer.RawValue.Should().Be("علی محمدی");
    }

    [Fact]
    public void Should_Extract_Customer_From_Payment()
    {
        var result = _extractor.Extract(
            "مهدی 500 هزار تومان پرداخت کرد");

        result.Should().ContainSingle();

        result.Single().RawValue
            .Should().Be("مهدی");
    }

    [Fact]
    public void Should_Return_Empty_When_Customer_Is_Not_Present()
    {
        var result = _extractor.Extract(
            "امروز 500 هزار تومان فروختم");

        result.Should().BeEmpty();
    }

    [Fact]
    public void Should_Not_Extract_Date_Word_As_Customer()
    {
        var result = _extractor.Extract(
            "فردا 500 هزار تومان فروختم");

        result.Should().BeEmpty();
    }

    [Fact]
    public void Should_Extract_Customer_Before_Money()
    {
        var result = _extractor.Extract(
            "علی 500 هزار تومان خرید");

        result.Should()
            .ContainSingle()
            .Which.RawValue
            .Should()
            .Be("علی");
    }
}