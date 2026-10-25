using Vexora.Domain.Constants.Subscriptions;
using Vexora.Domain.sharedKernel.Exceptions;

namespace Vexora.Domain.Exceptions.Subscriptions;

public static class SubscriptionExceptions
{
    public sealed class InvalidStartAndEndDate() : Exception
        (SubscriptionConstants.InvalidStartAndEndDate);

    public sealed class SubscriptionExpiredException() : AppException(
        "SUBSCRIPTION_EXPIRED",
        "اشتراک شما تمام شده است. می‌توانید اطلاعات را ببینید، ولی ثبت جدید ممکن نیست.",
        AppErrorType.PaymentRequired);
}
