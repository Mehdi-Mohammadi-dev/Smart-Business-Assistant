using Vexora.Domain.Enums.Conversations;
using Vexora.Domain.sharedKernel.Validators;

namespace Vexora.Domain.Entities.Conversations;

public sealed class Conversation
{
    public const int TitleLength = 60;

    private readonly List<ConversationMessage> _messages = [];

    public Guid Id { get; private set; }

    public long UserId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset LastMessageAt { get; private set; }

    public bool IsDelete { get; private set; }

    public DateTimeOffset? DeletedAt { get; private set; }

    public IReadOnlyCollection<ConversationMessage> Messages => _messages;

    private Conversation()
    {
    }

    public static Conversation Start(long userId, DateTimeOffset now)
    {
        Guard.AgainstInvalidId(userId);

        return new Conversation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CreatedAt = now,
            LastMessageAt = now
        };
    }

    public ConversationMessage AddMessage(
        MessageRole role,
        string text,
        DateTimeOffset now,
        string? payloadJson = null,
        CardState cardState = CardState.None)
    {
        var message = ConversationMessage.Create(
            Id,
            role,
            text,
            payloadJson,
            cardState,
            now);

        _messages.Add(message);

        LastMessageAt = now;

        if (role == MessageRole.User && string.IsNullOrEmpty(Title))
            Title = BuildTitle(message.Text);

        return message;
    }

    public void SoftDelete(DateTimeOffset now)
    {
        if (IsDelete)
            return;

        IsDelete = true;
        DeletedAt = now;
    }

    public static string BuildTitle(string text)
    {
        var singleLine = string.Join(
            ' ',
            text.Split(
                [' ', '\n', '\r', '\t'],
                StringSplitOptions.RemoveEmptyEntries));

        return singleLine.Length <= TitleLength
            ? singleLine
            : singleLine[..(TitleLength - 1)].TrimEnd() + "…";
    }
}
