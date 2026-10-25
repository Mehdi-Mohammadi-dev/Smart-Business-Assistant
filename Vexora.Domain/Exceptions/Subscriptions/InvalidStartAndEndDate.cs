using Vexora.Domain.Constants.Subscriptions;
using Vexora.Domain.sharedKernel.Exceptions;

namespace Vexora.Domain.Exceptions.Subscriptions;

public sealed class InvalidStartAndEndDate() : AppException(
"INVALID_SUBSCRIPTION_DATES",
SubscriptionConstants.InvalidStartAndEndDate,
AppErrorType.Validation);
