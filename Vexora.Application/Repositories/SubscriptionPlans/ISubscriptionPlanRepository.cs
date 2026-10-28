using Vexora.Domain.Entities.SubscriptionPlans;

namespace Vexora.Application.Repositories.SubscriptionPlans;    

public interface ISubscriptionPlanRepository
{
    Task AddAsync(SubscriptionPlan plan, CancellationToken ct = default);
    Task<SubscriptionPlan?> FindByIdAsync(long id, CancellationToken ct = default);
    Task<IReadOnlyList<SubscriptionPlan>> ListAsync(bool onlyActive, CancellationToken ct = default);
}
