using Vexora.Application.Repositories.Generics;
using Vexora.Domain.Entities.Customers;

namespace Vexora.Application.Repositories.Customers;

public interface ICustomerRepository
    : IGenericRepository<Customer>
{
    Task<IReadOnlyList<Customer>> SearchByNameAsync(
        long userId,
        string name,
        CancellationToken cancellationToken = default);

    Task<Customer?> GetByExactNameAsync(
        long userId,
        string firstName,
        string lastName,
        CancellationToken cancellationToken = default);
}
