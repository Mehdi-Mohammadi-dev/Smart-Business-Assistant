
namespace Vexora.Application.Exceptions.Authentications;

public sealed class OtpTooManyAttemptsException() : AppException(
"OTP_TOO_MANY_ATTEMPTS",
"تعداد تلاش‌های ناموفق زیاد است. لطفاً کد جدید دریافت کنید.",
AppErrorType.TooManyRequests);
