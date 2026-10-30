namespace Vexora.Api.Contracts.Admin.Referrals;

public sealed record ReferralResponse(
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
