namespace Vexora.Application.Exceptions;

public sealed class ForbiddenException() : AppException(
    "FORBIDDEN", "اجازه انجام این عملیات را ندارید.", AppErrorType.Forbidden);
