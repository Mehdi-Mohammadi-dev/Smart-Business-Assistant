
namespace Vexora.Application.Exceptions.Authentications;

public sealed class OtpInvalidException() : AppException(
"OTP_INVALID",
"کد تایید نادرست است.",
AppErrorType.Validation);
