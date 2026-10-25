using Vexora.Domain.sharedKernel.Exceptions;

namespace Vexora.Domain.Exceptions.Conversations;

public sealed class CardNotPendingException() : AppException(
"CARD_NOT_PENDING",
"این کارت دیگر قابل تأیید یا لغو نیست.",
AppErrorType.Conflict);
