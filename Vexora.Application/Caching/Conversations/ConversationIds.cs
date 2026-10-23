namespace Vexora.Application.Caching.Conversations;

public static class ConversationIds
{
    public static string ForUser(long userId)
    {
        if (userId <= 0)
            throw new ArgumentOutOfRangeException(nameof(userId));

        return $"user-{userId}";
    }
}
