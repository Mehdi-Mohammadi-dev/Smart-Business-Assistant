using Vexora.Domain.Constants.Checks;
using Vexora.Domain.sharedKernel.Exceptions;

namespace Vexora.Domain.Exceptions.Checks;

public sealed class CheckAlreadyPendingException() : AppException(
"CHECK_ALREADY_PENDING",
CheckMessages.AlreadyPending,
AppErrorType.Conflict);
