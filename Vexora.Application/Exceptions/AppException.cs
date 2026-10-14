namespace Vexora.Application.Exceptions;

public enum AppErrorType
{
    Validation,
    Unauthorized,
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
