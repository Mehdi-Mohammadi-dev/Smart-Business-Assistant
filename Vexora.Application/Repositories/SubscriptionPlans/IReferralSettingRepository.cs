using Vexora.Domain.Entities.SubscriptionPlans;

namespace Vexora.Application.Repositories.SubscriptionPlans;

public interface IReferralSettingRepository
{
    Task<ReferralSetting?> GetCurrentAsync(CancellationToken cancellationToken = default);
    Task AddAsync(ReferralSetting setting, CancellationToken cancellationToken = default);
}
