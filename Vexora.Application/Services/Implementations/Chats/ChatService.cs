using Vexora.Application.Constants.Chats;
using Vexora.Application.Conversations;
using Vexora.Application.Conversations.Abstracts;
using Vexora.Application.Dtos.Chats;
using Vexora.Application.Dtos.Customers;
using Vexora.Application.Dtos.Payments;
using Vexora.Application.NLU.Abstractions.Pipeline;
using Vexora.Application.NLU.Models.Entities;
using Vexora.Application.NLU.Models.Intents;
using Vexora.Application.NLU.Models.Money;
using Vexora.Application.NLU.Models.Results;
using Vexora.Application.Services.Interfaces.Chats;
using Vexora.Application.Services.Interfaces.Customers;
using Vexora.Application.Services.Interfaces.Payments;

namespace Vexora.Application.Services.Implementations.Chats;

public sealed class ChatService(
    INluPipeline nluPipeline,
    ICustomerService customerService,
    IConversationStateStore conversationStateStore,
    IPaymentService paymentService) : IChatService
{
    public async Task<ChatResponseDto> ProcessAsync(
        ChatRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var state =
            await conversationStateStore.GetAsync(
                request.ConversationId,
                cancellationToken);

        if (state is not null)
        {
            return await ContinueConversationAsync(
                state,
                request.Message,
                cancellationToken);
        }

        var nluResult =
            nluPipeline.Analyze(request.Message);

        return await HandleNewMessageAsync(
            request.ConversationId,
            nluResult,
            cancellationToken);
    }

    private async Task<ChatResponseDto> HandleNewMessageAsync(
        string conversationId,
        NluResult nluResult,
        CancellationToken cancellationToken)
    {
        if (nluResult.Intent == IntentType.Unknown)
        {
            return new ChatResponseDto
            (
               ChatsConstants.UnderstandrequestMessage,
               true, []
            );
        }

        if (nluResult.Intent != IntentType.RegisterPayment)
        {
            return new ChatResponseDto
            (
                ChatsConstants.NoImplimentationMvpMessage,
                false, []
            );
        }

        return await HandleRegisterPaymentAsync(
            conversationId,
            nluResult,
            cancellationToken);
    }

    private async Task<ChatResponseDto> HandleRegisterPaymentAsync(
    string conversationId,
    NluResult nluResult,
    CancellationToken cancellationToken)
    {
        var moneyEntity =
            GetEntity(nluResult, EntityType.Money);

        if (moneyEntity?.Value is not MoneyAmount money)
        {
            return new ChatResponseDto
            (
                "مبلغ پرداخت را مشخص کنید.",
                true, []
            );
        }

        var customerEntity =
            GetEntity(nluResult, EntityType.Customer);

        if (customerEntity?.Value is not CustomerMention customerMention)
        {
            return new ChatResponseDto
            (
                "لطفاً نام طرف حساب را مشخص کنید.",
                true, []
            );
        }

        var resolution =
            await customerService.ResolveByNameAsync(
                customerMention.RawValue,
                cancellationToken);

        if (resolution.Status ==
            CustomerResolutionStatus.Resolved)
        {
            await paymentService.RegisterAsync(
                new RegisterPaymentDto
                (
                    resolution.Customer!.CustomerId,
                    money.Amount, DateTime.Now
                ),
                cancellationToken);

            return new ChatResponseDto
            (
                    $"پرداخت {money.Amount:N0} تومان برای {resolution.Customer.DisplayName} ثبت شد.",
                    false, []
            );
        }

        if (resolution.Status ==
            CustomerResolutionStatus.Ambiguous)
        {
            var state = new ConversationState
            {
                ConversationId = conversationId,
                Status =
                    ConversationStatus.WaitingForCustomerSelection,

                PendingAction = new PendingAction
                (
                    IntentType.RegisterPayment,
                    customerMention.RawValue,
                    customerMention.Start,
                    money.Amount,
                    money.Unit,
                    DateTime.Now
                ),

                CustomerCandidates =
                    resolution.Candidates
            };

            await conversationStateStore.SaveAsync(
                state,
                cancellationToken);

            return new ChatResponseDto
            (
                     "چند طرف حساب با نام «{customerMention.RawValue}» پیدا شد. لطفاً یکی را انتخاب کنید.",
                     true,
                     resolution.Candidates
                    .Select(x => x.DisplayName)
                    .ToArray()
            );
        }

        var notFoundState = new ConversationState
        {
            ConversationId = conversationId,

            Status =
                ConversationStatus
                    .WaitingForCustomerCreationConfirmation,

            PendingAction = new PendingAction
            (
             IntentType.RegisterPayment,
             customerMention.RawValue,
             customerMention.Start,
             money.Amount,
             money.Unit,
             DateTime.Now
            )
        };

        await conversationStateStore.SaveAsync(
            notFoundState,
            cancellationToken);

        return new ChatResponseDto
        (
                $"طرف حسابی با نام «{customerMention.RawValue}» پیدا نکردم. آیا می‌خواهید آن را ایجاد کنم؟",
                true,
            [
                "بله",
            "خیر"
            ]
        );
    }

    private async Task<ChatResponseDto> ContinueConversationAsync(
    ConversationState state,
    string message,
    CancellationToken cancellationToken)
    {
        return state.Status switch
        {
            ConversationStatus.WaitingForCustomerSelection =>
                await HandleCustomerSelectionAsync(
                    state,
                    message,
                    cancellationToken),

            ConversationStatus.WaitingForCustomerCreationConfirmation =>
                await HandleCustomerCreationConfirmationAsync(
                    state,
                    message,
                    cancellationToken),

            _ => new ChatResponseDto
            (
                "لطفاً درخواست خود را دوباره ارسال کنید.",
                true, []
            )
        };
    }

    private static EntityResult? GetEntity(
    NluResult result,
    EntityType type)
    {
        return result.Entities
            .FirstOrDefault(x => x.Type == type);
    }

    private async Task<ChatResponseDto> HandleCustomerSelectionAsync(
    ConversationState state,
    string message,
    CancellationToken cancellationToken)
    {
        if (!int.TryParse(message.Trim(), out var selectedNumber))
        {
            return new ChatResponseDto
            (
                    "لطفاً شماره یکی از طرف حساب‌ها را وارد کنید.",
                true,

                 state.CustomerCandidates
                    .Select(x => x.DisplayName)
                    .ToArray()
            );
        }

        var candidates =
            state.CustomerCandidates.ToArray();

        if (selectedNumber < 1 ||
            selectedNumber > candidates.Length)
        {
            return new ChatResponseDto
            (
                    "شماره انتخاب‌شده معتبر نیست. لطفاً یکی از گزینه‌ها را انتخاب کنید.",
                true,

                candidates
                    .Select(x => x.DisplayName)
                    .ToArray()
            );
        }

        var selectedCustomer =
            candidates[selectedNumber - 1];

        var pending = state.PendingAction;

        if (pending is null ||
            pending.Intent != IntentType.RegisterPayment ||
            pending.Amount is null)
        {
            await conversationStateStore.RemoveAsync(
                state.ConversationId,
                cancellationToken);

            return new ChatResponseDto
            (
                    "اطلاعات درخواست قبلی کامل نیست. لطفاً درخواست را دوباره ارسال کنید.",
                true, []
            );
        }

        await paymentService.RegisterAsync(
            new RegisterPaymentDto
            (
                selectedCustomer.CustomerId,
                pending.Amount.Value,
                pending.Date
            ),
            cancellationToken);

        await conversationStateStore.RemoveAsync(
            state.ConversationId,
            cancellationToken);

        return new ChatResponseDto
        (
                $"پرداخت {pending.Amount.Value:N0} تومان برای {selectedCustomer.DisplayName} ثبت شد.",
            false, []
        );
    }

    private async Task<ChatResponseDto>
    HandleCustomerCreationConfirmationAsync(
        ConversationState state,
        string message,
        CancellationToken cancellationToken)
    {
        var normalizedMessage = message
            .Trim()
            .ToLowerInvariant();

        var isYes =
            normalizedMessage is
                "بله" or
                "آره" or
                "اره" or
                "بله حتما" or
                "yes" or
                "y";

        var isNo =
            normalizedMessage is
                "خیر" or
                "نه" or
                "نخیر" or
                "no" or
                "n";

        if (!isYes && !isNo)
        {
            return new ChatResponseDto
            (
                    "لطفاً «بله» یا «خیر» را انتخاب کنید.",
                true,
                [
                    "بله",
                "خیر"
                ]
            );
        }

        if (isNo)
        {
            await conversationStateStore.RemoveAsync(
                state.ConversationId,
                cancellationToken);

            return new ChatResponseDto
            (
                    "باشه، پرداخت ثبت نشد.",
                 false, []
            );
        }

        var pending = state.PendingAction;

        if (pending is null ||
            string.IsNullOrWhiteSpace(pending.CustomerName) ||
            pending.Amount is null ||
            pending.Amount <= 0)
        {
            await conversationStateStore.RemoveAsync(
                state.ConversationId,
                cancellationToken);

            return new ChatResponseDto
            (
                    "اطلاعات درخواست قبلی کامل نیست. لطفاً درخواست پرداخت را دوباره ارسال کنید.",
                true, []
            );
        }

        // فعلاً تا CreateCustomer را پیاده کنیم،
        // این قسمت را بعداً کامل می‌کنیم.
        return new ChatResponseDto
        (
                $"ایجاد طرف حساب «{pending.CustomerName}» در مرحله بعد انجام می‌شود.",
         false, []
        );
    }
}
