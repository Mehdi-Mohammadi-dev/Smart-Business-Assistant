namespace Vexora.Application.Exceptions.Subscriptions;

public sealed class SubscriptionExpiredException() : AppException(
    "SUBSCRIPTION_EXPIRED",
    "اشتراک شما تمام شده است. می‌توانید اطلاعات را ببینید، ولی ثبت جدید ممکن نیست.",
    AppErrorType.PaymentRequired);
