using Vexora.Application.Exceptions;

namespace Vexora.Application.Ledger.Exceptions;

public sealed class PaymentNotFoundException() : AppException(
"PAYMENT_NOT_FOUND",
"پرداخت پیدا نشد.",
AppErrorType.NotFound);
