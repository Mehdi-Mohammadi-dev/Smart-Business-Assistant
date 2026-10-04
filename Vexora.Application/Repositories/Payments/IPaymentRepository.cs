using Vexora.Application.Repositories.Generics;
using Vexora.Domain.Entities.Payments;

namespace Vexora.Application.Repositories.Payments;

public interface IPaymentRepository : IGenericRepository<Payment>
{
    Task<IReadOnlyList<Payment>> GetByCustomerAsync(
        long userId,
        long customerId,
        CancellationToken cancellationToken = default);
}