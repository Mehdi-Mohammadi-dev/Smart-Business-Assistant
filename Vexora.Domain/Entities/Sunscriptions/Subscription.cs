using Vexora.Domain.Entities.Users;
using Vexora.Domain.Enums.Subscriptions;
using Vexora.Domain.sharedKernel.BaseEntities;
using Vexora.Domain.sharedKernel.Validators;
using Vexora.Domain.Validators.Subscriptions;

namespace Vexora.Domain.Entities.Sunscriptions;

public class Subscription : UserBaseEntity
{
    public SubscriptionPlanType PlanType { get; private set; }
    public SubscriptionStatus Status { get; private set; }

    public DateTimeOffset StartAt { get; private set; }
    public DateTimeOffset EndAt { get; private set; }

    #region Relations

    public User User { get; private set; } = null!;

    #endregion

    #region Constractors

    private Subscription()
    {
    }

    #endregion

    #region Functions

    public static Subscription Create(
        long userId,
        SubscriptionPlanType planType,
        DateTimeOffset startAt,
        DateTimeOffset endAt)
    {

        Guard.AgainstInvalidId(userId);

        SubscriptionValidation.CheckStartAndEndDate(startAt, endAt);
        return new Subscription
        {
            UserId = userId,
            PlanType = planType,
            StartAt = startAt,
            EndAt = endAt,
            Status = SubscriptionStatus.Pending
        };
    }

    public void Activate()
    {
        if (Status == SubscriptionStatus.Cancelled)
            throw new InvalidOperationException(
                "A cancelled subscription cannot be activated.");
        Status = SubscriptionStatus.Active;
        
    }

    public void Cancel()
    {
        if (Status == SubscriptionStatus.Cancelled)
            return;

        Status = SubscriptionStatus.Cancelled;
        
    }

    public void Expire()
    {
        Status = SubscriptionStatus.Expired;
        
    }

    #endregion
}
