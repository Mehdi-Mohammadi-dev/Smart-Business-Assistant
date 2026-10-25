using Vexora.Application.Exceptions;

namespace Vexora.Application.Ledger.Exceptions;

public static class LedgerExceptions
{
    public sealed class CustomerNotFoundException() : AppException(
        "CUSTOMER_NOT_FOUND",
        "مشتری پیدا نشد.",
        AppErrorType.NotFound);

    public sealed class PaymentNotFoundException() : AppException(
        "PAYMENT_NOT_FOUND",
        "پرداخت پیدا نشد.",
        AppErrorType.NotFound);

    public sealed class CheckNotFoundException() : AppException(
        "CHECK_NOT_FOUND",
        "چک پیدا نشد.",
        AppErrorType.NotFound);

    public sealed class StatementTooLargeException() : AppException(
        "STATEMENT_TOO_LARGE",
        "بازه را کوچک‌تر کنید.",
        AppErrorType.Validation);

    public sealed class InvalidDateRangeException() : AppException(
        "INVALID_DATE_RANGE",
        "تاریخ شروع نمی‌تواند بعد از تاریخ پایان باشد.",
        AppErrorType.Validation);
}
