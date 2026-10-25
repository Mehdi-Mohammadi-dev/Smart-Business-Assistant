
namespace Vexora.Application.Exceptions.Authentications;

public sealed class PhoneNumberAlreadyExistsException() : AppException(
"PHONE_ALREADY_EXISTS",
"این شماره موبایل قبلاً ثبت‌نام کرده است.",
AppErrorType.Conflict);
