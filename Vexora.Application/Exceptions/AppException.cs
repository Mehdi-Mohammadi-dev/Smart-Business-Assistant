namespace Vexora.Application.Exceptions;

// Kept as an application-facing enum for source compatibility. Application
// exceptions are adapted to the shared Domain AppException so the API
// middleware handles both Domain and Application failures consistently.
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
    : Vexora.Domain.sharedKernel.Exceptions.AppException(
        code,
        message,
        ToDomainErrorType(type))
{
    private static Vexora.Domain.sharedKernel.Exceptions.AppErrorType ToDomainErrorType(
        AppErrorType type) => type switch
        {
            AppErrorType.Validation => Vexora.Domain.sharedKernel.Exceptions.AppErrorType.Validation,
            AppErrorType.Unauthorized => Vexora.Domain.sharedKernel.Exceptions.AppErrorType.Unauthorized,
            AppErrorType.PaymentRequired => Vexora.Domain.sharedKernel.Exceptions.AppErrorType.PaymentRequired,
            AppErrorType.Forbidden => Vexora.Domain.sharedKernel.Exceptions.AppErrorType.Forbidden,
            AppErrorType.NotFound => Vexora.Domain.sharedKernel.Exceptions.AppErrorType.NotFound,
            AppErrorType.Conflict => Vexora.Domain.sharedKernel.Exceptions.AppErrorType.Conflict,
            AppErrorType.TooManyRequests => Vexora.Domain.sharedKernel.Exceptions.AppErrorType.TooManyRequests,
            AppErrorType.ExternalService => Vexora.Domain.sharedKernel.Exceptions.AppErrorType.ExternalService,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown application error type.")
        };
}
