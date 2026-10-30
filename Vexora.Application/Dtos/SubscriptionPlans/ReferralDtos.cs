namespace Vexora.Application.Dtos.SubscriptionPlans;

public sealed record ReferralRowDto(
    long ReferralId,
    long ReferrerUserId,
    string ReferrerName,
    string ReferrerPhoneNumber,
    string ReferrerInviteCode,
    long ReferredUserId,
    string ReferredName,
    string ReferredPhoneNumber,
    DateTimeOffset RegisteredAt,
    int TotalReferredByReferrer,
    bool RewardGranted,
    int RewardDays,
    bool HasPurchasedSubscription,
    bool SubscriptionActive,
    string? SubscriptionPlanType,
    DateTimeOffset? SubscriptionEndAt);

public sealed record ReferralSettingDto(bool IsEnabled, int RewardDays, int MaxRewardDaysPerUser);
