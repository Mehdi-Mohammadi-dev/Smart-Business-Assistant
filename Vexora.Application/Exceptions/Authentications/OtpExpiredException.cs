
namespace Vexora.Application.Exceptions.Authentications;

public sealed class OtpExpiredException() : AppException(
"OTP_EXPIRED",
"کد تایید منقضی شده است. لطفاً کد جدید دریافت کنید.",
AppErrorType.Validation);
