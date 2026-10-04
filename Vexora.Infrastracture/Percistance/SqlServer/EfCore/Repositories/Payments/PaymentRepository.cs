using Microsoft.EntityFrameworkCore;
using Vexora.Application.Repositories.Payments;
using Vexora.Domain.Entities.Checks;
using Vexora.Domain.Entities.Payments;
using Vexora.Infrastracture.Percistance.SqlServer.EfCore.Context;
using Vexora.Infrastracture.Percistance.SqlServer.EfCore.Repositories.Generics;

namespace Vexora.Infrastracture.Percistance.SqlServer.EfCore.Repositories.Payments;

public sealed class PaymentRepository(ApplicationDbContext context):GenericRepository<Payment>(context),IPaymentRepository
{
    public async Task<IReadOnlyList<Payment>> GetByCustomerAsync(
        long userId,
        long customerId,
        CancellationToken cancellationToken = default)
    {
        return await context.Payments
            .AsNoTracking()
            .Where(x =>
                x.UserId == userId &&
                x.CustomerId == customerId)
            .OrderByDescending(x => x.PaymentDate)
            .ToListAsync(cancellationToken);
    }
}
