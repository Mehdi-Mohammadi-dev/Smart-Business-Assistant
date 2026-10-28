namespace Vexora.Application.Exceptions.SubscriptionPlans;

public sealed class InvalidSubscriptionPlanException(string message)
    : AppException("INVALID_PLAN", message, AppErrorType.Validation);
