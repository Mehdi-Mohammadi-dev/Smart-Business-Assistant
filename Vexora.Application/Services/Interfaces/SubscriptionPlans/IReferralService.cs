using Vexora.Application.Dtos.SubscriptionPlans;

namespace Vexora.Application.Services.Interfaces.SubscriptionPlans;

public interface IReferralService
{
    Task<IReadOnlyList<ReferralRowDto>> ListAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<ReferralSettingDto> GetSettingsAsync(CancellationToken cancellationToken = default);
    Task<ReferralSettingDto> UpdateSettingsAsync(ReferralSettingDto dto, CancellationToken cancellationToken = default);
}
