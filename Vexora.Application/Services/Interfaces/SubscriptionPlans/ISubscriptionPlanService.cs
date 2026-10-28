using Vexora.Application.Dtos.SubscriptionPlans;

namespace Vexora.Application.Services.Interfaces.SubscriptionPlans;

public interface ISubscriptionPlanService
{
    Task<IReadOnlyList<SubscriptionPlanDto>> GetActiveAsync(CancellationToken ct = default);
    Task<IReadOnlyList<SubscriptionPlanDto>> GetAllAsync(CancellationToken ct = default);
    Task<SubscriptionPlanDto> CreateAsync(SaveSubscriptionPlanDto dto, CancellationToken ct = default);
    Task<SubscriptionPlanDto> UpdateAsync(long id, SaveSubscriptionPlanDto dto, CancellationToken ct = default);
    Task DeactivateAsync(long id, CancellationToken ct = default);
}
