
namespace Vexora.Application.Exceptions.Authentications;

public sealed class LoginTemporarilyLockedException() : AppException(
"AUTH_TEMPORARILY_LOCKED",
"ورود موقتاً قفل شده است. لطفاً ۱۰ دقیقه دیگر تلاش کنید.",
AppErrorType.TooManyRequests);
