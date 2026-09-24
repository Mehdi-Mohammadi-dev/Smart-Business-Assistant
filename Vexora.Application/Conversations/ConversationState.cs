using Vexora.Application.Dtos.Customers;

namespace Vexora.Application.Conversations;

public sealed class ConversationState
{
    public required string ConversationId { get; init; }

    public ConversationStatus Status { get; set; }

    public PendingAction? PendingAction { get; set; }

    public IReadOnlyCollection<CustomerCandidateDto> CustomerCandidates
    {
        get;
        set;
    } = [];
}
