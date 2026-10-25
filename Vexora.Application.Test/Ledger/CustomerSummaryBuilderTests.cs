using FluentAssertions;
using Vexora.Application.Ledger.Customers;
using Vexora.Application.Ledger.Models;
using Xunit;

namespace Vexora.Application.Test.Ledger;

public sealed class CustomerSummaryBuilderTests
{
    private static readonly DateTimeOffset Early = new(2026, 10, 1, 9, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Late = new(2026, 10, 20, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public void TotalReceived_IsCashPlusPaidChecksOnly()
    {
        var customers = new[] { new CustomerRef(1, "علی رضایی") };

        var payments = new[] { new PaymentAggregate { CustomerId = 1, Total = 3_500_000m, LastAt = Early } };

        var checks = new[]
        {
            new CheckAggregate
            {
                CustomerId = 1,
                PendingTotal = 20_000_000m,
                PendingCount = 2,
                OverdueCount = 1,
                PaidTotal = 2_000_000m,
                LastAt = Late
            }
        };

        var summary = CustomerSummaryBuilder.Build(customers, payments, checks).Single();

        summary.CashReceived.Should().Be(3_500_000m);
        summary.ChecksPendingTotal.Should().Be(20_000_000m);
        summary.ChecksPendingCount.Should().Be(2);
        summary.ChecksOverdueCount.Should().Be(1);
        summary.TotalReceived.Should().Be(5_500_000m);
        summary.LastActivityAt.Should().Be(Late);
    }

    [Fact]
    public void CustomerWithoutActivity_HasZerosAndNullLastActivity()
    {
        var summary = CustomerSummaryBuilder.Build(
            [new CustomerRef(1, "مریم احمدی")], [], []).Single();

        summary.CashReceived.Should().Be(0);
        summary.TotalReceived.Should().Be(0);
        summary.ChecksPendingCount.Should().Be(0);
        summary.LastActivityAt.Should().BeNull();
    }

    [Fact]
    public void Sort_LastActivity_NewestFirst_NoActivityLast()
    {
        var summaries = CustomerSummaryBuilder.Build(
            [new CustomerRef(1, "الف"), new CustomerRef(2, "ب"), new CustomerRef(3, "پ")],
            [new PaymentAggregate { CustomerId = 1, Total = 1, LastAt = Early }],
            [new CheckAggregate { CustomerId = 2, LastAt = Late }]);

        CustomerSummaryBuilder.Sort(summaries, CustomerSort.LastActivity)
            .Select(x => x.Id).Should().Equal(2, 1, 3);
    }

    [Fact]
    public void Sort_Name_FollowsPersianAlphabet()
    {
        var summaries = CustomerSummaryBuilder.Build(
            [
                new CustomerRef(1, "یاسر کریمی"),
                new CustomerRef(2, "علی رضایی"),
                new CustomerRef(3, "پریسا یوسفی"),
                new CustomerRef(4, "ابراهیم نوری")
            ], [], []);

        CustomerSummaryBuilder.Sort(summaries, CustomerSort.Name)
            .Select(x => x.Id).Should().Equal(4, 3, 2, 1);
    }
}
