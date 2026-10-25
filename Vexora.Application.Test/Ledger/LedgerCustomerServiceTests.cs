using FluentAssertions;
using Moq;
using Vexora.Application.Ledger.Customers;
using Vexora.Application.Ledger.Exceptions;
using Vexora.Application.Ledger.Models;
using Vexora.Application.Ledger.Repositories;
using Vexora.Application.Services.Interfaces.CurrentUser;
using Xunit;

namespace Vexora.Application.Test.Ledger;

public sealed class LedgerCustomerServiceTests
{
    private sealed class FakeReader : ILedgerCustomerReader
    {
        public readonly Dictionary<long, List<CustomerRef>> CustomersByUser = new();
        public string? LastSearch;

        public Task<IReadOnlyList<CustomerRef>> FindCustomersAsync(
            long userId, string? normalizedSearch, long? customerId, CancellationToken cancellationToken = default)
        {
            LastSearch = normalizedSearch;

            var list = CustomersByUser.GetValueOrDefault(userId) ?? [];

            IEnumerable<CustomerRef> query = list;

            if (customerId is { } id)
                query = query.Where(x => x.Id == id);

            if (normalizedSearch is not null)
                query = query.Where(x => x.FullName.Contains(normalizedSearch));

            return Task.FromResult<IReadOnlyList<CustomerRef>>(query.ToList());
        }

        public Task<IReadOnlyList<PaymentAggregate>> GetPaymentAggregatesAsync(
            long userId, long? customerId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<PaymentAggregate>>([]);

        public Task<IReadOnlyList<CheckAggregate>> GetCheckAggregatesAsync(
            long userId, long? customerId, DateTime today, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<CheckAggregate>>([]);

        public Task<int> CountCustomersAsync(long userId, CancellationToken cancellationToken = default)
            => Task.FromResult(CustomersByUser.GetValueOrDefault(userId)?.Count ?? 0);
    }

    private static LedgerCustomerService Create(FakeReader reader, long userId)
    {
        var currentUser = new Mock<ICurrentUser>();
        currentUser.Setup(x => x.UserId).Returns(userId);

        return new LedgerCustomerService(
            reader,
            currentUser.Object,
            new LedgerFixedClock(new DateTimeOffset(2026, 10, 24, 9, 0, 0, TimeSpan.Zero)));
    }

    private static FakeReader TwoUsers()
    {
        var reader = new FakeReader();
        reader.CustomersByUser[1] = Enumerable.Range(1, 25).Select(i => new CustomerRef(i, $"مشتری {i}")).ToList();
        reader.CustomersByUser[2] = [new CustomerRef(100, "مشتری دیگران")];
        return reader;
    }

    [Fact]
    public async Task List_ReturnsOnlyCurrentUsersCustomers()
    {
        var result = await Create(TwoUsers(), 2).ListAsync(new CustomerListQuery(null, CustomerSort.Name, 1, 20));

        result.Items.Select(x => x.Id).Should().Equal(100);
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task List_PagesResults()
    {
        var service = Create(TwoUsers(), 1);

        var page2 = await service.ListAsync(new CustomerListQuery(null, CustomerSort.Name, 2, 20));

        page2.Items.Should().HaveCount(5);
        page2.TotalCount.Should().Be(25);
        page2.Page.Should().Be(2);
        page2.PageSize.Should().Be(20);
    }

    [Fact]
    public async Task List_PassesNormalizedSearchToReader()
    {
        var reader = TwoUsers();

        await Create(reader, 1).ListAsync(new CustomerListQuery("  مشتري   ۱ ", CustomerSort.Name, 1, 20));

        reader.LastSearch.Should().Be("مشتری 1");
    }

    [Fact]
    public async Task Get_OtherUsersCustomer_IsNotFound()
    {
        var act = () => Create(TwoUsers(), 1).GetAsync(100);

        await act.Should().ThrowAsync<LedgerExceptions.CustomerNotFoundException>();
    }

    [Fact]
    public async Task Get_OwnCustomer_ReturnsSummary()
    {
        var summary = await Create(TwoUsers(), 1).GetAsync(3);

        summary.Id.Should().Be(3);
        summary.FullName.Should().Be("مشتری 3");
    }
}
