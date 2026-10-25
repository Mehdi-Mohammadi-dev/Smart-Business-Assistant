using Vexora.Domain.Constants.Checks;
using Vexora.Domain.sharedKernel.Exceptions;

namespace Vexora.Domain.Exceptions.Checks;

public static class CheckExceptions
{
    public sealed class CheckNotPendingException() : AppException(
        "CHECK_NOT_PENDING",
        CheckMessages.NotPending,
        AppErrorType.Conflict);

    public sealed class CheckAlreadyPendingException() : AppException(
        "CHECK_ALREADY_PENDING",
        CheckMessages.AlreadyPending,
        AppErrorType.Conflict);
}
