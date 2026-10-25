using Vexora.Domain.sharedKernel.Exceptions;

namespace Vexora.Domain.Exceptions.Conversations;

public static class ConversationExceptions
{
    public sealed class CardNotPendingException() : AppException(
        "CARD_NOT_PENDING",
        "این کارت دیگر قابل تأیید یا لغو نیست.",
        AppErrorType.Conflict);

    public sealed class ConversationNotFoundException() : AppException(
        "CONVERSATION_NOT_FOUND",
        "گفتگو پیدا نشد.",
        AppErrorType.NotFound);

    public sealed class MessageNotFoundException() : AppException(
        "MESSAGE_NOT_FOUND",
        "پیام پیدا نشد.",
        AppErrorType.NotFound);
}
