using Vexora.Domain.sharedKernel.Exceptions;

namespace Vexora.Domain.Exceptions.Conversations;

public sealed class MessageNotFoundException() : AppException(
"MESSAGE_NOT_FOUND",
"پیام پیدا نشد.",
AppErrorType.NotFound);
