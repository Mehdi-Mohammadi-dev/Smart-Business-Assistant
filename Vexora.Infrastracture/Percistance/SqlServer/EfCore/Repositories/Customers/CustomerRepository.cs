using Microsoft.EntityFrameworkCore;
using Vexora.Application.Repositories.Customers;
using Vexora.Domain.Entities.Customers;
using Vexora.Infrastracture.Percistance.SqlServer.EfCore.Context;
using Vexora.Infrastracture.Percistance.SqlServer.EfCore.Repositories.Generics;

namespace Vexora.Infrastracture.Percistance.SqlServer.EfCore.Repositories.Customers;

public class CustomerRepository(ApplicationDbContext context) : GenericRepository<Customer>(context), ICustomerRepository
{
    public async Task<IReadOnlyList<Customer>> SearchByNameAsync(long userId, string name, CancellationToken cancellationToken = default)
    {
        name = name.Trim();
        return await context.Customers.Where(x => x.UserId == userId &&
                (
                    x.FirstName.Contains(name) ||
                    x.LastName.Contains(name) ||
                    (x.FirstName + " " + x.LastName).Contains(name)
                ))
            .ToListAsync(cancellationToken);
    }

    public async Task<Customer?> GetByExactNameAsync(
        long userId,
        string firstName,
        string lastName,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = CustomerNameNormalizer.Normalize(firstName, lastName);

        var customers = await context.Customers
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);

        return customers.FirstOrDefault(x =>
            CustomerNameNormalizer.Normalize(x.FirstName, x.LastName) == normalizedName);

    }
}
