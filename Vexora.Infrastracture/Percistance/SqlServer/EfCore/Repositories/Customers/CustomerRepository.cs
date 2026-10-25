using Microsoft.EntityFrameworkCore;
using Vexora.Application.Repositories.Customers;
using Vexora.Domain.Entities.Customers;
using Vexora.Infrastracture.Percistance.SqlServer.EfCore.Context;
using Vexora.Infrastracture.Percistance.SqlServer.EfCore.Repositories.Generics;

namespace Vexora.Infrastracture.Percistance.SqlServer.EfCore.Repositories.Customers;

public class CustomerRepository(ApplicationDbContext context) : GenericRepository<Customer>(context), ICustomerRepository
{
    public async Task<IReadOnlyList<Customer>> SearchByNameAsync(
        long userId,
        string normalizedName,
        CancellationToken cancellationToken = default)
    {
        return await context.Customers
            .Where(x => x.UserId == userId &&
                        x.NormalizedName.Contains(normalizedName))
            .OrderBy(x => x.NormalizedName)
            .ToListAsync(cancellationToken);
    }

    public async Task<Customer?> GetByNormalizedNameAsync(
        long userId,
        string normalizedName,
        CancellationToken cancellationToken = default)
    {
        return await context.Customers
            .FirstOrDefaultAsync(
                x => x.UserId == userId &&
                     x.NormalizedName == normalizedName,
                cancellationToken);
    }
}
