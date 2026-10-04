using Microsoft.EntityFrameworkCore;
using Vexora.Application.Repositories.Checks;
using Vexora.Domain.Entities.Checks;
using Vexora.Infrastracture.Percistance.SqlServer.EfCore.Context;
using Vexora.Infrastracture.Percistance.SqlServer.EfCore.Repositories.Generics;

namespace Vexora.Infrastracture.Percistance.SqlServer.EfCore.Repositories.Checks;

public sealed class CheckRepository(ApplicationDbContext context):GenericRepository<Check>(context),ICheckRepository
{
    public async Task<IReadOnlyList<Check>> GetByCustomerAsync(
    long userId,
    long customerId,
    CancellationToken cancellationToken = default)
    {
        return await context.Checks
            .AsNoTracking()
            .Where(x =>
                x.UserId == userId &&
                x.CustomerId == customerId)
            .OrderByDescending(x => x.DueDate)
            .ToListAsync(cancellationToken);
    }
}
