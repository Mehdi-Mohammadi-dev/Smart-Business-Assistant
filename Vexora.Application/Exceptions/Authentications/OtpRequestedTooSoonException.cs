
namespace Vexora.Application.Exceptions.Authentications;

public sealed class OtpRequestedTooSoonException() : AppException(
"OTP_REQUESTED_TOO_SOON",
"لطفاً کمی صبر کنید و دوباره درخواست کد بدهید.",
AppErrorType.TooManyRequests);
