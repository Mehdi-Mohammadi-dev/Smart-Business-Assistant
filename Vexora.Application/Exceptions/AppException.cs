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

// Base class for expected (business) errors. The API middleware turns these into
// proper HTTP status codes, so we never need a new "case" for each new exception.
public abstract class AppException(string code, string message, AppErrorType type)
    : Exception(message)
{
    public string Code { get; } = code;
    public AppErrorType Type { get; } = type;
}
