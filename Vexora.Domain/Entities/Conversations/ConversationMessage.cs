using Vexora.Domain.Enums.Conversations;
using Vexora.Domain.Exceptions.Conversations;

namespace Vexora.Domain.Entities.Conversations;

public sealed class ConversationMessage
{
    public Guid Id { get; private set; }

    public Guid ConversationId { get; private set; }

    public MessageRole Role { get; private set; }

    public string Text { get; private set; } = string.Empty;

    public string? PayloadJson { get; private set; }

    public CardState CardState { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public Conversation Conversation { get; private set; } = null!;

    public bool IsPending => CardState == CardState.Pending;

    private ConversationMessage()
    {
    }

    internal static ConversationMessage Create(
        Guid conversationId,
        MessageRole role,
        string text,
        string? payloadJson,
        CardState cardState,
        DateTimeOffset createdAt)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Message text is required.", nameof(text));

        return new ConversationMessage
        {
            Id = Guid.NewGuid(),
            ConversationId = conversationId,
            Role = role,
            Text = text.Trim(),
            PayloadJson = payloadJson,
            CardState = cardState,
            CreatedAt = createdAt
        };
    }

    public void Confirm() => Move(CardState.Confirmed);

    public void Cancel() => Move(CardState.Cancelled);

    public void Expire() => Move(CardState.Expired);

    private void Move(CardState next)
    {
        if (CardState != CardState.Pending)
            throw new ConversationExceptions.CardNotPendingException();

        CardState = next;
    }
}
