using Vexora.Domain.Constants.Subscriptions;
using Vexora.Domain.sharedKernel.Exceptions;

namespace Vexora.Domain.Exceptions.Subscriptions;

public sealed class SubscriptionExpiredException() : AppException(
"SUBSCRIPTION_EXPIRED",
"اشتراک فعال ندارید. ثبت اطلاعات جدید امکان‌پذیر نیست.",
AppErrorType.PaymentRequired);
