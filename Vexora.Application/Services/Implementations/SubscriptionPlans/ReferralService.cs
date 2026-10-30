using Vexora.Application.Dtos.SubscriptionPlans;
using Vexora.Application.Repositories.SubscriptionPlans;
using Vexora.Application.Repositories.Subscriptions;
using Vexora.Application.Repositories.Users;
using Vexora.Application.Services.Interfaces.SubscriptionPlans;
using Vexora.Application.UnitOfWorks;
using Vexora.Domain.Entities.SubscriptionPlans;

namespace Vexora.Application.Services.Implementations.SubscriptionPlans;

public sealed class ReferralService(
    IReferralRepository referrals,
    IReferralSettingRepository settings,
    IUserRepository users,
    ISubscriptionRepository subscriptions,
    IUnitOfWork unitOfWork) : IReferralService
{
    public async Task<IReadOnlyList<ReferralRowDto>> ListAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var rows = await referrals.ListAsync(page, pageSize, cancellationToken);
        return await MapRowsAsync(rows, cancellationToken);
    }

    public async Task<IReadOnlyList<ReferralRowDto>> ListByReferrerAsync(long referrerUserId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var rows = await referrals.ListByReferrerAsync(referrerUserId, page, pageSize, cancellationToken);
        return await MapRowsAsync(rows, cancellationToken);
    }

    private async Task<IReadOnlyList<ReferralRowDto>> MapRowsAsync(
        IReadOnlyList<Referral> rows,
        CancellationToken cancellationToken)
    {
        var counts = await referrals.GetReferralCountsAsync(cancellationToken);
        var result = new List<ReferralRowDto>(rows.Count);
        foreach (var row in rows)
        {
            var referrer = await users.FindByIdAsync(row.ReferrerUserId, cancellationToken);
            var referred = await users.FindByIdAsync(row.ReferredUserId, cancellationToken);
            if (referrer is null || referred is null) continue;

            var purchasedSubscription = await subscriptions.GetLatestPurchasedByUserIdAsync(referred.Id, cancellationToken);
            var subscriptionActive = purchasedSubscription?.IsActiveAt(DateTimeOffset.UtcNow) == true;

            result.Add(new ReferralRowDto(
                row.Id, referrer.Id, referrer.DisplayName, referrer.PhoneNumber,
                referrer.InviteCode, referred.Id, referred.DisplayName, referred.PhoneNumber, referred.CreateAt,
                counts.GetValueOrDefault(referrer.Id), row.RewardGranted, row.RewardDays,
                purchasedSubscription is not null, subscriptionActive,
                purchasedSubscription?.PlanType.ToString(), purchasedSubscription?.EndAt));
        }
        return result;
    }

    public async Task<ReferralSettingDto> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        var setting = await settings.GetCurrentAsync(cancellationToken);
        return setting is null ? new ReferralSettingDto(false, 0, 0) :
            new ReferralSettingDto(setting.IsEnabled, setting.RewardDays, setting.MaxRewardDaysPerUser);
    }

    public async Task<ReferralSettingDto> UpdateSettingsAsync(ReferralSettingDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.RewardDays < 0 || dto.MaxRewardDaysPerUser < 0)
            throw new ArgumentOutOfRangeException(nameof(dto), "مقادیر روزهای پاداش نمی‌توانند منفی باشند.");
        var setting = await settings.GetCurrentAsync(cancellationToken);
        if (setting is null)
        {
            setting = ReferralSetting.Create(dto.IsEnabled, dto.RewardDays, dto.MaxRewardDaysPerUser);
            await settings.AddAsync(setting, cancellationToken);
        }
        else setting.Update(dto.IsEnabled, dto.RewardDays, dto.MaxRewardDaysPerUser);
        await unitOfWork.SaveAsync(cancellationToken);
        return new ReferralSettingDto(setting.IsEnabled, setting.RewardDays, setting.MaxRewardDaysPerUser);
    }
}
