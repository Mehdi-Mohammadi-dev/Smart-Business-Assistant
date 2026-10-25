using Vexora.Domain.Exceptions.Subscriptions;

namespace Vexora.Domain.Validators.Subscriptions;

public static class SubscriptionValidation
{
    public static void CheckStartAndEndDate(DateTimeOffset startAt, DateTimeOffset endAt)
    {
        if (endAt <= startAt)
            throw new InvalidStartAndEndDate();
    }
}
