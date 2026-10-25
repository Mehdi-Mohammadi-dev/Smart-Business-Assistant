using Vexora.Domain.sharedKernel.Exceptions;

namespace Vexora.Domain.Exceptions.Checks;

public static class CheckExceptions
{
    public sealed class CheckNotPendingException() : AppException(
        "CHECK_NOT_PENDING",
        "این چک در وضعیت «در انتظار» نیست. اگر اشتباه ثبت کرده‌اید، اول آن را به «در انتظار» برگردانید.",
        AppErrorType.Conflict);

    public sealed class CheckAlreadyPendingException() : AppException(
        "CHECK_ALREADY_PENDING",
        "این چک از قبل در وضعیت «در انتظار» است.",
        AppErrorType.Conflict);
}
