
namespace Vexora.Application.Exceptions.Authentications;

public sealed class InvalidCredentialsException() : AppException(
"INVALID_CREDENTIALS",
"شماره موبایل یا رمز عبور نادرست است.",
AppErrorType.Unauthorized);
