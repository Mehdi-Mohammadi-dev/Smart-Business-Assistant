using Vexora.Api.Contracts.Admin.Referrals;
using Vexora.Application.Dtos.SubscriptionPlans;

namespace Vexora.Api.Mappers.Admin;

public static class ReferralMapper
{
    public static ReferralResponse ToContract(this ReferralRowDto dto) => new(
        dto.ReferralId, dto.ReferrerUserId, dto.ReferrerName, dto.ReferrerPhoneNumber,
        dto.ReferrerInviteCode, dto.ReferredUserId, dto.ReferredName, dto.ReferredPhoneNumber,
        dto.RegisteredAt, dto.TotalReferredByReferrer, dto.RewardGranted, dto.RewardDays,
        dto.HasPurchasedSubscription, dto.SubscriptionActive, dto.SubscriptionPlanType, dto.SubscriptionEndAt);
}
