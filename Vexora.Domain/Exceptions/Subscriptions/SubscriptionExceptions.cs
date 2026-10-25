using Vexora.Domain.Constants.Subscriptions;
using Vexora.Domain.sharedKernel.Exceptions;

namespace Vexora.Domain.Exceptions.Subscriptions;

public static class SubscriptionExceptions
{
    public sealed class InvalidStartAndEndDate() : AppException(
        "INVALID_SUBSCRIPTION_DATES",
        SubscriptionConstants.InvalidStartAndEndDate,
        AppErrorType.Validation);

    public sealed class SubscriptionExpiredException() : AppException(
        "SUBSCRIPTION_EXPIRED",
        "اشتراک فعال ندارید. ثبت اطلاعات جدید امکان‌پذیر نیست.",
        AppErrorType.PaymentRequired);
}
