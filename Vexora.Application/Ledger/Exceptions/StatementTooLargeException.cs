using Vexora.Application.Exceptions;

namespace Vexora.Application.Ledger.Exceptions;

public sealed class StatementTooLargeException() : AppException(
"STATEMENT_TOO_LARGE",
"بازه را کوچک‌تر کنید.",
AppErrorType.Validation);
