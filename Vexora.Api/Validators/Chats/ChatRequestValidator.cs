using FluentValidation;
using Vexora.Api.Constants.Shared;
using Vexora.Api.Contracts.Chats;

namespace Vexora.Api.Validators.Chats;

public sealed class ChatRequestValidator
    : AbstractValidator<ChatRequest>
{
    public ChatRequestValidator()
    {
        RuleFor(x => x.ConversationId)
            .NotEmpty()
            .WithMessage(Messages.ConversationIdRequired);

        RuleFor(x => x.Message)
            .NotEmpty()
            .WithMessage(Messages.MessageRequired)
            .MaximumLength(2000)
            .WithMessage(Messages.MessageMaxLength);
    }
}
