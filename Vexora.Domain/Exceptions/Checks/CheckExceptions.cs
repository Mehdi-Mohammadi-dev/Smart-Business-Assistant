using Vexora.Domain.Constants.Checks;

namespace Vexora.Domain.Exceptions.Checks;

public static class CheckExceptions
{
    public sealed class CheckNotPendingException() : Exception
        (CheckMessages.NotPending);

    public sealed class CheckAlreadyPendingException() : Exception
        (CheckMessages.AlreadyPending);
}
