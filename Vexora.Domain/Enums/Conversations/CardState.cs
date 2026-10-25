namespace Vexora.Domain.Enums.Conversations;

public enum CardState : byte
{
    None = 0,
    Pending = 1,
    Confirmed = 2,
    Cancelled = 3,
    Expired = 4
}
