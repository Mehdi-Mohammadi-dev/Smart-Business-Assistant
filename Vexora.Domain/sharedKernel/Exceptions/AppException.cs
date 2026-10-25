namespace Vexora.Domain.sharedKernel.Exceptions;

public enum AppErrorType
{
    Validation,
    Unauthorized,
    PaymentRequired,
    Forbidden,
    NotFound,
    Conflict,
    TooManyRequests,
    ExternalService
}

public abstract class AppException(string code, string message, AppErrorType type)
    : Exception(message)
{
    public string Code { get; } = code;
    public AppErrorType Type { get; } = type;
}
