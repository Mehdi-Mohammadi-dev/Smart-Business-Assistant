using Vexora.Domain.Constants.Subscriptions;

namespace Vexora.Domain.Exceptions.Subscriptions;

public static class SubscriptionExceptions
{
    public sealed class InvalidStartAndEndDate() : Exception
        (SubscriptionConstants.InvalidStartAndEndDate);
}
