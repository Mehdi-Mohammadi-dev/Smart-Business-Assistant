using Vexora.Application.Exceptions;

namespace Vexora.Application.Ledger.Exceptions;

public sealed class CheckNotFoundException() : AppException(
"CHECK_NOT_FOUND",
"چک پیدا نشد.",
AppErrorType.NotFound);
