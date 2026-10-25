
namespace Vexora.Application.Exceptions.Authentications;

public sealed class WeakPasswordException() : AppException(
"WEAK_PASSWORD", "رمز عبور باید حداقل ۸ نویسه باشد.", AppErrorType.Validation);
