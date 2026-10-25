using Vexora.Domain.sharedKernel.Exceptions;

namespace Vexora.Domain.Exceptions.Conversations;

public sealed class ConversationNotFoundException() : AppException(
"CONVERSATION_NOT_FOUND",
"گفتگو پیدا نشد.",
AppErrorType.NotFound);
