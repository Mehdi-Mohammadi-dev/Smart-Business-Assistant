using Vexora.Application.Caching.Conversations;
using Vexora.Application.Caching.Conversations.Interfaces;
using Vexora.Application.Caching.Conversations.Mappers.CustomerConversations;
using Vexora.Application.Constants.Messages.Conversations;
using Vexora.Application.Dtos.Chats;
using Vexora.Application.Dtos.Checks;
using Vexora.Application.Dtos.Customers;
using Vexora.Application.Dtos.Payments;
using Vexora.Application.NLU.Abstractions.Dates;
using Vexora.Application.NLU.Abstractions.Pipeline;
using Vexora.Application.NLU.Models.Entities;
using Vexora.Application.NLU.Models.Intents;
using Vexora.Application.NLU.Models.Money;
using Vexora.Application.NLU.Models.Results;
using Vexora.Application.Services.Interfaces.Checks;
using Vexora.Application.Services.Interfaces.Customers;
using Vexora.Application.Services.Interfaces.Payments;

namespace Vexora.Application.Caching.Conversations.Implementations;

public sealed class ConversationProcessor(
    INluPipeline nluPipeline,
    IConversationStateStore conversationStateStore,
    ICustomerService customerService,
    IPaymentService paymentService,
    ICheckService checkService,
    IDateExtractor dateExtractor)
    : IConversationProcessor
{
    public async Task<ConversationProcessingResult> ProcessAsync(
        ChatRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

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

        return await ProcessNewMessageAsync(
            request.ConversationId,
            nluResult,
            cancellationToken);
    }

    private async Task<ConversationProcessingResult>
        ProcessNewMessageAsync(
            string conversationId,
            NluResult nluResult,
            CancellationToken cancellationToken)
    {
        return nluResult.Intent switch
        {
            IntentType.Unknown =>
                new ConversationProcessingResult(
                    ProcessingStatus.Failed,
                    ConversationMessages.UnknownRequest,
                    null),

            IntentType.RegisterPayment =>
                await ProcessRegisterPaymentAsync(
                    conversationId,
                    nluResult,
                    cancellationToken),

            IntentType.RegisterCheck =>
                await ProcessRegisterCheckAsync(
                    conversationId,
                    nluResult,
                    cancellationToken),

            _ =>
                new ConversationProcessingResult(
                    ProcessingStatus.Success,
                    ConversationMessages.RequestRecognized,
                    nluResult)
        };
    }

    private async Task<ConversationProcessingResult>
        ProcessRegisterPaymentAsync(
            string conversationId,
            NluResult nluResult,
            CancellationToken cancellationToken)
    {
        var customerEntity =
            nluResult.Entities.FirstOrDefault(
                x => x.Type == EntityType.Customer);

        var moneyEntity =
            nluResult.Entities.FirstOrDefault(
                x => x.Type == EntityType.Money);

        if (customerEntity?.Value is not CustomerMention customer)
        {
            return new ConversationProcessingResult(
                ProcessingStatus.Failed,
                ConversationMessages.CustomerNameRequired,
                null);
        }

        if (moneyEntity?.Value is not MoneyAmount money)
        {
            return new ConversationProcessingResult(
                ProcessingStatus.Failed,
                ConversationMessages.PaymentAmountRequired,
                null);
        }

        var resolution =
            await customerService.ResolveByNameAsync(
                customer.RawValue,
                cancellationToken);

        if (resolution.Status ==
            CustomerResolutionStatus.Ambiguous)
        {
            var pendingAction = new PendingAction(
                IntentType.RegisterPayment,
                customer.RawValue,
                null,
                money.Amount,
                money.Unit,
                DateTime.Now);

            await SaveCustomerSelectionStateAsync(
                conversationId,
                pendingAction,
                resolution.Candidates,
                cancellationToken);

            return new ConversationProcessingResult(
                ProcessingStatus.Failed,
                ConversationMessages.CustomerSelectionRequired,
                resolution.Candidates);
        }

        if (resolution.Status ==
            CustomerResolutionStatus.NotFound)
        {
            var pendingAction = new PendingAction(
                IntentType.RegisterPayment,
                customer.RawValue,
                null,
                money.Amount,
                money.Unit,
                DateTime.Now);

            await SaveCustomerCreationConfirmationStateAsync(
                conversationId,
                pendingAction,
                cancellationToken);

            return new ConversationProcessingResult(
                ProcessingStatus.Failed,
                ConversationMessages.CustomerNotFound(
                    customer.RawValue),
                null);
        }

        if (resolution.Customer is null)
        {
            return new ConversationProcessingResult(
                ProcessingStatus.Failed,
                ConversationMessages.CustomerInformationInvalid,
                null);
        }

        await paymentService.RegisterAsync(
            new RegisterPaymentDto(
                resolution.Customer.CustomerId,
                money.Amount,
                DateTime.Now),
            cancellationToken);

        return new ConversationProcessingResult(
            ProcessingStatus.Success,
            ConversationMessages.PaymentRegistered(
                money.Amount,
                resolution.Customer.DisplayName),
            null);
    }

    private async Task<ConversationProcessingResult>
        ProcessRegisterCheckAsync(
            string conversationId,
            NluResult nluResult,
            CancellationToken cancellationToken)
    {
        var customerEntity =
            nluResult.Entities.FirstOrDefault(
                x => x.Type == EntityType.Customer);

        var moneyEntity =
            nluResult.Entities.FirstOrDefault(
                x => x.Type == EntityType.Money);

        var dateEntity =
            nluResult.Entities.FirstOrDefault(
                x => x.Type == EntityType.Date);

        if (customerEntity?.Value is not CustomerMention customer)
        {
            return new ConversationProcessingResult(
                ProcessingStatus.Failed,
                ConversationMessages.CustomerNameRequired,
                null);
        }

        if (moneyEntity?.Value is not MoneyAmount money)
        {
            return new ConversationProcessingResult(
                ProcessingStatus.Failed,
                ConversationMessages.CheckAmountRequired,
                null);
        }

        var resolution =
            await customerService.ResolveByNameAsync(
                customer.RawValue,
                cancellationToken);

        if (resolution.Status ==
            CustomerResolutionStatus.Ambiguous)
        {
            var pendingAction = new PendingAction(
                IntentType.RegisterCheck,
                customer.RawValue,
                null,
                money.Amount,
                money.Unit,
                dateEntity?.Value as DateTime?);

            await SaveCustomerSelectionStateAsync(
                conversationId,
                pendingAction,
                resolution.Candidates,
                cancellationToken);

            return new ConversationProcessingResult(
                ProcessingStatus.Failed,
                ConversationMessages.CustomerSelectionRequired,
                resolution.Candidates);
        }

        if (resolution.Status ==
            CustomerResolutionStatus.NotFound)
        {
            var pendingAction = new PendingAction(
                IntentType.RegisterCheck,
                customer.RawValue,
                null,
                money.Amount,
                money.Unit,
                dateEntity?.Value as DateTime?);

            await SaveCustomerCreationConfirmationStateAsync(
                conversationId,
                pendingAction,
                cancellationToken);

            return new ConversationProcessingResult(
                ProcessingStatus.Failed,
                ConversationMessages.CustomerNotFound(
                    customer.RawValue),
                null);
        }

        if (resolution.Customer is null)
        {
            return new ConversationProcessingResult(
                ProcessingStatus.Failed,
                ConversationMessages.CustomerInformationInvalid,
                null);
        }

        if (dateEntity?.Value is not DateTime date)
        {
            var pendingAction = new PendingAction(
                IntentType.RegisterCheck,
                resolution.Customer.DisplayName,
                resolution.Customer.CustomerId,
                money.Amount,
                money.Unit,
                null);

            await conversationStateStore.SaveAsync(
                new ConversationState
                {
                    ConversationId = conversationId,
                    Status = ConversationStatus.WaitingForCheckDate,
                    PendingAction = pendingAction
                },
                cancellationToken);

            return new ConversationProcessingResult(
                ProcessingStatus.Failed,
                ConversationMessages.CheckDueDateRequired,
                null);
        }

        await checkService.RegisterAsync(
            new RegisterCheckDto(
                resolution.Customer.CustomerId,
                money.Amount,
                date,
                null),
            cancellationToken);

        return new ConversationProcessingResult(
            ProcessingStatus.Success,
            ConversationMessages.CheckRegistered(
                money.Amount,
                resolution.Customer.DisplayName),
            null);
    }

    private async Task<ConversationProcessingResult>
        ContinueConversationAsync(
            ConversationState state,
            string message,
            CancellationToken cancellationToken)
    {
        return state.Status switch
        {
            ConversationStatus.WaitingForCheckDate =>
                await HandleCheckDateAsync(
                    state,
                    message,
                    cancellationToken),

            ConversationStatus.WaitingForCustomerCreationConfirmation =>
                await HandleCustomerCreationConfirmationAsync(
                    state,
                    message,
                    cancellationToken),

            ConversationStatus.WaitingForCustomerSelection =>
                await HandleCustomerSelectionAsync(
                    state,
                    message,
                    cancellationToken),

            ConversationStatus.WaitingForCustomerName =>
                await HandleNewCustomerNameAsync(
                    state,
                    message,
                    cancellationToken),

            _ =>
                new ConversationProcessingResult(
                    ProcessingStatus.Failed,
                    ConversationMessages.ConversationContinuationUnknown,
                    null)
        };
    }

    private async Task<ConversationProcessingResult>
        HandleCheckDateAsync(
            ConversationState state,
            string message,
            CancellationToken cancellationToken)
    {
        var date = dateExtractor.Extract(message);

        if (date is null)
        {
            return new ConversationProcessingResult(
                ProcessingStatus.Failed,
                ConversationMessages.InvalidDate,
                null);
        }

        var action = state.PendingAction;

        if (action is null ||
            action.CustomerId is null ||
            action.Amount is null)
        {
            await conversationStateStore.RemoveAsync(
                state.ConversationId,
                cancellationToken);

            return new ConversationProcessingResult(
                ProcessingStatus.Failed,
                ConversationMessages.CheckInformationNotFound,
                null);
        }

        await checkService.RegisterAsync(
            new RegisterCheckDto(
                action.CustomerId.Value,
                action.Amount.Value,
                date.Value,
                action.CheckNumber),
            cancellationToken);

        await conversationStateStore.RemoveAsync(
            state.ConversationId,
            cancellationToken);

        return new ConversationProcessingResult(
            ProcessingStatus.Success,
            ConversationMessages.CheckRegistered(
                action.Amount.Value,
                action.CustomerName ?? string.Empty),
            null);
    }

    private async Task<ConversationProcessingResult>
        HandleNewCustomerNameAsync(
            ConversationState state,
            string message,
            CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return new ConversationProcessingResult(
                ProcessingStatus.Failed,
                ConversationMessages.CustomerNameRequired,
                null);
        }

        var pendingAction = state.PendingAction;

        if (pendingAction is null)
        {
            await conversationStateStore.RemoveAsync(
                state.ConversationId,
                cancellationToken);

            return new ConversationProcessingResult(
                ProcessingStatus.Failed,
                ConversationMessages.PreviousActionNotFound,
                null);
        }

        var customerDto =
            CustomerConversationMapper.ToCreateCustomerDto(
                message);

        var customer =
            await customerService.CreateAsync(
                customerDto,
                cancellationToken);

        var customerDisplayName =
            BuildDisplayName(
                customer.FirstName,
                customer.LastName);

        var updatedPendingAction =
            pendingAction with
            {
                CustomerId = customer.CustomerId,
                CustomerName = customerDisplayName
            };

        return await CompletePendingActionAsync(
            state,
            updatedPendingAction,
            cancellationToken);
    }

    private async Task<ConversationProcessingResult>
        HandleCustomerSelectionAsync(
            ConversationState state,
            string message,
            CancellationToken cancellationToken)
    {
        if (!int.TryParse(
                message.Trim(),
                out var selection))
        {
            return new ConversationProcessingResult(
                ProcessingStatus.Failed,
                ConversationMessages.CustomerSelectionRequired,
                state.CustomerCandidates);
        }

        var candidates =
            state.CustomerCandidates.ToArray();

        var createCustomerOption =
            candidates.Length + 1;

        if (selection == createCustomerOption)
        {
            state.Status =
                ConversationStatus.WaitingForCustomerName;

            await conversationStateStore.SaveAsync(
                state,
                cancellationToken);

            return new ConversationProcessingResult(
                ProcessingStatus.Failed,
                ConversationMessages.CustomerNameEnter,
                null);
        }

        if (selection < 1 ||
            selection > candidates.Length)
        {
            return new ConversationProcessingResult(
                ProcessingStatus.Failed,
                ConversationMessages.InvalidCustomerSelection,
                candidates);
        }

        var selectedCustomer =
            candidates[selection - 1];

        var pendingAction =
            state.PendingAction;

        if (pendingAction is null)
        {
            await conversationStateStore.RemoveAsync(
                state.ConversationId,
                cancellationToken);

            return new ConversationProcessingResult(
                ProcessingStatus.Failed,
                ConversationMessages.PreviousActionNotFound,
                null);
        }

        var updatedPendingAction =
            pendingAction with
            {
                CustomerId = selectedCustomer.CustomerId,
                CustomerName = selectedCustomer.DisplayName
            };

        return await CompletePendingActionAsync(
            state,
            updatedPendingAction,
            cancellationToken);
    }

    private async Task<ConversationProcessingResult>
        HandleCustomerCreationConfirmationAsync(
            ConversationState state,
            string message,
            CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return new ConversationProcessingResult(
                ProcessingStatus.Failed,
                ConversationMessages.CustomerCreationResponseRequired,
                null);
        }

        if (IsYes(message))
        {
            return await CreateCustomerAndCompleteActionAsync(
                state,
                cancellationToken);
        }

        if (IsNo(message))
        {
            await conversationStateStore.RemoveAsync(
                state.ConversationId,
                cancellationToken);

            return new ConversationProcessingResult(
                ProcessingStatus.Success,
                ConversationMessages.CustomerCreationCancelled,
                null);
        }

        return new ConversationProcessingResult(
            ProcessingStatus.Failed,
            ConversationMessages.YesOrNoRequired,
            null);
    }

    private async Task<ConversationProcessingResult>
        CreateCustomerAndCompleteActionAsync(
            ConversationState state,
            CancellationToken cancellationToken)
    {
        var pendingAction = state.PendingAction;

        if (pendingAction is null ||
            string.IsNullOrWhiteSpace(
                pendingAction.CustomerName))
        {
            await conversationStateStore.RemoveAsync(
                state.ConversationId,
                cancellationToken);

            return new ConversationProcessingResult(
                ProcessingStatus.Failed,
                ConversationMessages.CustomerInformationNotFound,
                null);
        }

        var createCustomerDto =
            CustomerConversationMapper.ToCreateCustomerDto(
                pendingAction.CustomerName);

        var customer =
            await customerService.CreateAsync(
                createCustomerDto,
                cancellationToken);

        var customerDisplayName =
            BuildDisplayName(
                customer.FirstName,
                customer.LastName);

        var updatedPendingAction =
            pendingAction with
            {
                CustomerId = customer.CustomerId,
                CustomerName = customerDisplayName
            };

        return await CompletePendingActionAsync(
            state,
            updatedPendingAction,
            cancellationToken);
    }

    private async Task<ConversationProcessingResult>
        CompletePendingActionAsync(
            ConversationState state,
            PendingAction pendingAction,
            CancellationToken cancellationToken)
    {
        if (pendingAction.Intent == IntentType.RegisterPayment)
        {
            if (!pendingAction.Amount.HasValue)
            {
                await conversationStateStore.RemoveAsync(
                    state.ConversationId,
                    cancellationToken);

                return new ConversationProcessingResult(
                    ProcessingStatus.Failed,
                    ConversationMessages.PaymentAmountNotFound,
                    null);
            }

            await paymentService.RegisterAsync(
                new RegisterPaymentDto(
                    pendingAction.CustomerId!.Value,
                    pendingAction.Amount.Value,
                    pendingAction.Date),
                cancellationToken);

            await conversationStateStore.RemoveAsync(
                state.ConversationId,
                cancellationToken);

            return new ConversationProcessingResult(
                ProcessingStatus.Success,
                ConversationMessages.PaymentRegistered(
                    pendingAction.Amount.Value,
                    pendingAction.CustomerName ?? string.Empty),
                null);
        }

        if (pendingAction.Intent == IntentType.RegisterCheck)
        {
            if (!pendingAction.Amount.HasValue)
            {
                await conversationStateStore.RemoveAsync(
                    state.ConversationId,
                    cancellationToken);

                return new ConversationProcessingResult(
                    ProcessingStatus.Failed,
                    ConversationMessages.CheckAmountNotFound,
                    null);
            }

            if (!pendingAction.CustomerId.HasValue)
            {
                await conversationStateStore.RemoveAsync(
                    state.ConversationId,
                    cancellationToken);

                return new ConversationProcessingResult(
                    ProcessingStatus.Failed,
                    ConversationMessages.CustomerInformationNotFound,
                    null);
            }

            if (!pendingAction.Date.HasValue)
            {
                state.PendingAction =
                    pendingAction;

                state.Status =
                    ConversationStatus.WaitingForCheckDate;

                await conversationStateStore.SaveAsync(
                    state,
                    cancellationToken);

                return new ConversationProcessingResult(
                    ProcessingStatus.Failed,
                    ConversationMessages.CheckDueDateRequired,
                    null);
            }

            await checkService.RegisterAsync(
                new RegisterCheckDto(
                    pendingAction.CustomerId.Value,
                    pendingAction.Amount.Value,
                    pendingAction.Date.Value,
                    pendingAction.CheckNumber),
                cancellationToken);

            await conversationStateStore.RemoveAsync(
                state.ConversationId,
                cancellationToken);

            return new ConversationProcessingResult(
                ProcessingStatus.Success,
                ConversationMessages.CheckRegistered(
                    pendingAction.Amount.Value,
                    pendingAction.CustomerName ?? string.Empty),
                null);
        }

        await conversationStateStore.RemoveAsync(
            state.ConversationId,
            cancellationToken);

        return new ConversationProcessingResult(
            ProcessingStatus.Failed,
            ConversationMessages.UnsupportedPendingAction,
            null);
    }

    private async Task SaveCustomerSelectionStateAsync(
        string conversationId,
        PendingAction pendingAction,
        IReadOnlyCollection<CustomerCandidateDto> candidates,
        CancellationToken cancellationToken)
    {
        await conversationStateStore.SaveAsync(
            new ConversationState
            {
                ConversationId = conversationId,
                Status =
                    ConversationStatus.WaitingForCustomerSelection,
                PendingAction = pendingAction,
                CustomerCandidates = candidates
            },
            cancellationToken);
    }

    private async Task SaveCustomerCreationConfirmationStateAsync(
        string conversationId,
        PendingAction pendingAction,
        CancellationToken cancellationToken)
    {
        await conversationStateStore.SaveAsync(
            new ConversationState
            {
                ConversationId = conversationId,
                Status =
                    ConversationStatus.WaitingForCustomerCreationConfirmation,
                PendingAction = pendingAction,
                CustomerCandidates = []
            },
            cancellationToken);
    }

    private static string BuildDisplayName(
        string firstName,
        string? lastName)
    {
        return $"{firstName} {lastName}".Trim();
    }

    private static bool IsYes(string message)
    {
        var normalized = message.Trim();

        return normalized is
            "بله" or
            "بلی" or
            "آره" or
            "اره" or
            "حتماً" or
            "حتما" or
            "باشه" or
            "اوکی" or
            "yes";
    }

    private static bool IsNo(string message)
    {
        var normalized = message.Trim();

        return normalized is
            "خیر" or
            "نه" or
            "نخیر" or
            "لغو" or
            "بیخیال" or
            "بی‌خیال" or
            "no";
    }
}