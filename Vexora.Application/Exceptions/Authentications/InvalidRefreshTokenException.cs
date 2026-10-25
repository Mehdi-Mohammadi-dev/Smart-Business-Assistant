
namespace Vexora.Application.Exceptions.Authentications;

public sealed class InvalidRefreshTokenException() : AppException(
"INVALID_REFRESH_TOKEN",
"توکن شما منقضی شده است. لطفاً دوباره وارد شوید.",
AppErrorType.Unauthorized);
