using Vexora.Domain.Constants.Checks;
using Vexora.Domain.sharedKernel.Exceptions;

namespace Vexora.Domain.Exceptions.Checks;

public sealed class CheckNotPendingException() : AppException(
"CHECK_NOT_PENDING",
CheckMessages.NotPending,
AppErrorType.Conflict);
