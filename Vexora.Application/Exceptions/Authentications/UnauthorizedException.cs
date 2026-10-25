
namespace Vexora.Application.Exceptions.Authentications;

public sealed class UnauthorizedException() : AppException(
"UNAUTHORIZED",
"لطفاً وارد حساب کاربری خود شوید.",
AppErrorType.Unauthorized);
