using Vexora.Domain.sharedKernel.Exceptions;

namespace Vexora.Application.Exceptions.Authentications;

public static class AuthExceptions
{
    public sealed class PhoneNumberAlreadyExistsException() : AppException(
        "PHONE_ALREADY_EXISTS",
        "این شماره موبایل قبلاً ثبت‌نام کرده است.",
        AppErrorType.Conflict);

    public sealed class OtpInvalidException() : AppException(
        "OTP_INVALID",
        "کد تایید نادرست است.",
        AppErrorType.Validation);

    public sealed class OtpExpiredException() : AppException(
        "OTP_EXPIRED",
        "کد تایید منقضی شده است. لطفاً کد جدید دریافت کنید.",
        AppErrorType.Validation);

    public sealed class OtpTooManyAttemptsException() : AppException(
        "OTP_TOO_MANY_ATTEMPTS",
        "تعداد تلاش‌های ناموفق زیاد است. لطفاً کد جدید دریافت کنید.",
        AppErrorType.TooManyRequests);

    public sealed class OtpRequestedTooSoonException() : AppException(
        "OTP_REQUESTED_TOO_SOON",
        "لطفاً کمی صبر کنید و دوباره درخواست کد بدهید.",
        AppErrorType.TooManyRequests);

    public sealed class InvalidCredentialsException() : AppException(
        "INVALID_CREDENTIALS",
        "شماره موبایل یا رمز عبور نادرست است.",
        AppErrorType.Unauthorized);

    public sealed class InvalidRefreshTokenException() : AppException(
        "INVALID_REFRESH_TOKEN",
        "توکن شما منقضی شده است. لطفاً دوباره وارد شوید.",
        AppErrorType.Unauthorized);

    public sealed class UnauthorizedException() : AppException(
        "UNAUTHORIZED",
        "لطفاً وارد حساب کاربری خود شوید.",
        AppErrorType.Unauthorized);
}
