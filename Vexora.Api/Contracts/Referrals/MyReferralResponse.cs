namespace Vexora.Api.Contracts.Referrals;

public sealed record MyReferralResponse(
    long ReferredUserId,
    string ReferredName,
    string ReferredPhoneNumber,
    DateTimeOffset RegisteredAt,
    bool HasPurchasedSubscription,
    bool SubscriptionActive,
    string? SubscriptionPlanType,
    DateTimeOffset? SubscriptionEndAt);
