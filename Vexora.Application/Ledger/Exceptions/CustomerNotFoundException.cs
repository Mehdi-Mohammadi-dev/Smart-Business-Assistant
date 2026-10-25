using Vexora.Application.Exceptions;

namespace Vexora.Application.Ledger.Exceptions;

public sealed class CustomerNotFoundException() : AppException(
"CUSTOMER_NOT_FOUND",
"مشتری پیدا نشد.",
AppErrorType.NotFound);
