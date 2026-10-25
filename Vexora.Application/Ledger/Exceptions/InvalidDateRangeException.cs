using Vexora.Application.Exceptions;

namespace Vexora.Application.Ledger.Exceptions;

public sealed class InvalidDateRangeException() : AppException(
"INVALID_DATE_RANGE",
"تاریخ شروع نمی‌تواند بعد از تاریخ پایان باشد.",
AppErrorType.Validation);
