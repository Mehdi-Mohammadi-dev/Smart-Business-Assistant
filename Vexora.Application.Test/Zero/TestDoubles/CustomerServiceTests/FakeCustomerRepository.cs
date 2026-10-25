using FluentAssertions;
using Vexora.Application.Dtos.Customers;
using Vexora.Application.Repositories.Customers;
using Vexora.Application.Services.Implementations.Customers;
using Vexora.Application.Services.Interfaces.CurrentUser;
using Vexora.Application.UnitOfWorks;
using Vexora.Domain.Entities.Customers;
using Xunit;

namespace Vexora.Application.Test.Zero.CustomerServiceTestsHelpers;

internal sealed class FakeCustomerRepository : ICustomerRepository
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

    public Task<Customer?> GetByExactNameAsync(
 long userId,
 string firstName,
 string lastName,
 CancellationToken cancellationToken = default)
    {
        var normalizedName =
            CustomerNameNormalizer.Normalize(firstName, lastName);

        var customer = Items.FirstOrDefault(x =>
            x.UserId == userId &&
            CustomerNameNormalizer.Normalize(x.FirstName, x.LastName)
                == normalizedName);

        return Task.FromResult(customer);
    }
}
