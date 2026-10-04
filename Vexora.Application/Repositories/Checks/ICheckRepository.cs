using Vexora.Application.Repositories.Generics;
using Vexora.Domain.Entities.Checks;

namespace Vexora.Application.Repositories.Checks;

public interface ICheckRepository:IGenericRepository<Check>
{
    Task<IReadOnlyList<Check>> GetByCustomerAsync(
     long userId,
     long customerId,
     CancellationToken cancellationToken = default);
}
