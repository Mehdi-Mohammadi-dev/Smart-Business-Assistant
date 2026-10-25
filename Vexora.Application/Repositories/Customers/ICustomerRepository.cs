using Vexora.Application.Repositories.Generics;
using Vexora.Domain.Entities.Customers;

namespace Vexora.Application.Repositories.Customers;

public interface ICustomerRepository
    : IGenericRepository<Customer>
{
    Task<IReadOnlyList<Customer>> SearchByNameAsync(
        long userId,
        string normalizedName,
        CancellationToken cancellationToken = default);

    Task<Customer?> GetByNormalizedNameAsync(
        long userId,
        string normalizedName,
        CancellationToken cancellationToken = default);
}
