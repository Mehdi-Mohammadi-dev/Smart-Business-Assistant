using FluentAssertions;
using Vexora.Application.Dtos.Customers;
using Vexora.Application.Repositories.Customers;
using Vexora.Application.Services.Implementations.Customers;
using Vexora.Application.Services.Interfaces.CurrentUser;
using Vexora.Application.UnitOfWorks;
using Vexora.Domain.Entities.Customers;
using Xunit;

namespace Vexora.Application.Test.Zero;

public sealed class CustomerServiceTests
{
    private const long UserId = 1;

    private readonly FakeCustomerRepository _repository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly CustomerService _service;

    public CustomerServiceTests()
    {
        _service = new CustomerService(_repository, new FakeCurrentUser(UserId), _unitOfWork);
    }

    private void Has(params string[] fullNames)
    {
        foreach (var fullName in fullNames)
        {
            var parts = fullName.Split(' ', 2);
            _repository.Items.Add(Customer.Create(parts[0], parts.Length > 1 ? parts[1] : "", UserId));
        }
    }

    [Fact]
    public async Task Exact_Name_Should_Win_Even_When_A_Longer_Name_Exists()
    {
        Has("علی رضایی", "علی رضایی بنا");

        var result = await _service.ResolveByNameAsync("علی رضایی");

        result.Status.Should().Be(CustomerResolutionStatus.Resolved);
        result.Customer!.DisplayName.Should().Be("علی رضایی");
    }

    [Fact]
    public async Task The_Longer_Name_Should_Resolve_To_Itself()
    {
        Has("علی رضایی", "علی رضایی بنا");

        var result = await _service.ResolveByNameAsync("علی رضایی بنا");

        result.Customer!.DisplayName.Should().Be("علی رضایی بنا");
    }

    [Fact]
    public async Task Different_Arabic_Letters_Should_Still_Match()
    {
        Has("علی کریمی");

        var result = await _service.ResolveByNameAsync("علي كريمي");

        result.Status.Should().Be(CustomerResolutionStatus.Resolved);
    }

    [Fact]
    public async Task A_First_Name_Shared_By_Two_People_Should_Be_Ambiguous()
    {
        Has("علی رضایی", "علی احمدی");

        var result = await _service.ResolveByNameAsync("علی");

        result.Status.Should().Be(CustomerResolutionStatus.Ambiguous);
        result.Customer.Should().BeNull();
        result.Candidates.Should().HaveCount(2);
    }

    [Fact]
    public async Task One_Partial_Match_Should_Resolve_To_That_Customer()
    {
        Has("علی رضایی", "رضا احمدی");

        var result = await _service.ResolveByNameAsync("رضایی");

        result.Status.Should().Be(CustomerResolutionStatus.Resolved);
        result.Customer!.DisplayName.Should().Be("علی رضایی");
    }

    [Fact]
    public async Task Unknown_Name_Should_Be_Not_Found()
    {
        Has("علی رضایی");

        var result = await _service.ResolveByNameAsync("حسین");

        result.Status.Should().Be(CustomerResolutionStatus.NotFound);
    }

    [Fact]
    public async Task A_Blank_Name_Should_Be_Not_Found()
    {
        Has("علی رضایی");

        var result = await _service.ResolveByNameAsync("   ");

        result.Status.Should().Be(CustomerResolutionStatus.NotFound);
    }

    [Fact]
    public async Task Another_Users_Customers_Should_Never_Be_Found()
    {
        _repository.Items.Add(Customer.Create("علی", "رضایی", 2));

        var result = await _service.ResolveByNameAsync("علی رضایی");

        result.Status.Should().Be(CustomerResolutionStatus.NotFound);
    }

    [Fact]
    public async Task Creating_A_New_Customer_Should_Save_It()
    {
        var result = await _service.CreateAsync(new CreateCustomerDto("حسین", "احمدی"));

        result.FirstName.Should().Be("حسین");
        _repository.Items.Should().HaveCount(1);
        _unitOfWork.SaveCount.Should().Be(1);
    }

    [Fact]
    public async Task Creating_The_Same_Name_Again_Should_Return_The_Existing_One()
    {
        Has("علی رضایی");

        await _service.CreateAsync(new CreateCustomerDto("علي", "رضايي"));

        _repository.Items.Should().HaveCount(1);
        _unitOfWork.SaveCount.Should().Be(0);
    }

    private sealed class FakeCurrentUser(long userId) : ICurrentUser
    {
        public long UserId { get; } = userId;
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveCount { get; private set; }

        public Task<int> SaveAsync(CancellationToken cancellationToken = default)
        {
            SaveCount++;
            return Task.FromResult(1);
        }

        public Task ExecuteInTransactionAsync(
            Func<CancellationToken, Task> action,
            CancellationToken cancellationToken = default)
            => action(cancellationToken);
    }

    private sealed class FakeCustomerRepository : ICustomerRepository
    {
        public List<Customer> Items { get; } = [];

        public Task<Customer?> GetByIdAsync(long userId, long id, CancellationToken cancellationToken = default)
            => Task.FromResult(Items.FirstOrDefault(x => x.UserId == userId && x.Id == id));

        public Task InsertAsync(Customer entity, CancellationToken cancellationToken = default)
        {
            Items.Add(entity);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<Customer>> SearchByNameAsync(
            long userId,
            string normalizedName,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Customer>>(
                Items.Where(x => x.UserId == userId && x.NormalizedName.Contains(normalizedName)).ToList());

        public Task<Customer?> GetByNormalizedNameAsync(
            long userId,
            string normalizedName,
            CancellationToken cancellationToken = default)
            => Task.FromResult(
                Items.FirstOrDefault(x => x.UserId == userId && x.NormalizedName == normalizedName));

        public Task<Customer?> GetByExactNameAsync(long userId, string firstName, string lastName, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
