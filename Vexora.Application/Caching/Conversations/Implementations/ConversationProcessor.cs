using Vexora.Application.NLU.Intent;
using Vexora.Application.Caching.Conversations;
using Vexora.Application.Caching.Conversations.Interfaces;
using Vexora.Application.Caching.Conversations.Mappers.CustomerConversations;
using Vexora.Application.Constants.Messages.Conversations;
using Vexora.Application.Dtos.Chats;
using Vexora.Application.Dtos.Checks;
using Vexora.Application.Dtos.Customers;
using Vexora.Application.Dtos.Payments;
using Vexora.Application.Ledger.Customers;
using Vexora.Application.NLU.Abstractions.Dates;
using Vexora.Application.NLU.Abstractions.Pipeline;
using Vexora.Application.NLU.Models.Entities;
using Vexora.Application.NLU.Models.Intents;
using Vexora.Application.NLU.Models.Money;
using Vexora.Application.NLU.Models.Results;
using Vexora.Application.Services.Interfaces.Checks;
using Vexora.Application.Services.Interfaces.Customers;
using Vexora.Application.Services.Interfaces.Payments;
using Vexora.Domain.Enums.BankRegistrationStatuses;

namespace Vexora.Application.Caching.Conversations.Implementations;

public sealed class ConversationProcessor(
    INluPipeline nluPipeline,
    IConversationStateStore conversationStateStore,
    ICustomerService customerService,
    IPaymentService paymentService,
    ICheckService checkService,
    IDateExtractor dateExtractor,
    ILedgerCustomerService ledgerCustomerService)
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
            if (IsCancelMessage(request.Message))
            {
                await conversationStateStore.RemoveAsync(
                    request.ConversationId,
                    cancellationToken);

                return new ConversationProcessingResult(
                    ProcessingStatus.Success,
                    ConversationMessages.ConversationCancelled,
                    null);
            }

            var newCommand =
                nluPipeline.Analyze(request.Message);

            var keepWaitingForAmount =
                state.Status == ConversationStatus.WaitingForAmount &&
                !newCommand.Entities.Any(x => x.Type == EntityType.Customer);

            if (!keepWaitingForAmount && IsNewCommand(newCommand))
            {
                await conversationStateStore.RemoveAsync(
                    request.ConversationId,
                    cancellationToken);

                return await ProcessNewMessageAsync(
                    request.ConversationId,
                    newCommand,
                    cancellationToken);
            }

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

    public async Task<ConversationProcessingResult?> GetPendingAsync(
        string conversationId,
        CancellationToken cancellationToken = default)
    {
        var state =
            await conversationStateStore.GetAsync(
                conversationId,
                cancellationToken);

        if (state is null)
            return null;

        return state.Status switch
        {
            ConversationStatus.WaitingForCheckDate =>
                new ConversationProcessingResult(
                    ProcessingStatus.Failed,
                    ConversationMessages.CheckDueDateRequired,
                    null),

            ConversationStatus.WaitingForAmount =>
                new ConversationProcessingResult(
                    ProcessingStatus.Failed,
                    AmountRequiredMessage(state.PendingAction),
                    null),

            ConversationStatus.WaitingForCheckRegistration =>
                new ConversationProcessingResult(
                    ProcessingStatus.Failed,
                    ConversationMessages.CheckRegistrationQuestion,
                    null),

            ConversationStatus.WaitingForCustomerSelection =>
                new ConversationProcessingResult(
                    ProcessingStatus.Failed,
                    ConversationMessages.CustomerSelectionRequired,
                    state.CustomerCandidates),

            ConversationStatus.WaitingForCustomerCreationConfirmation =>
                new ConversationProcessingResult(
                    ProcessingStatus.Failed,
                    ConversationMessages.CustomerNotFound(
                        state.PendingAction?.CustomerName ?? string.Empty),
                    null),

            ConversationStatus.WaitingForCustomerName =>
                new ConversationProcessingResult(
                    ProcessingStatus.Failed,
                    ConversationMessages.CustomerNameEnter,
                    null),

            _ => null
        };
    }

    public Task ResetAsync(
        string conversationId,
        CancellationToken cancellationToken = default)
    {
        return conversationStateStore.RemoveAsync(
            conversationId,
            cancellationToken);
    }

    private async Task<ConversationProcessingResult>
        ProcessNewMessageAsync(
            string conversationId,
            NluResult nluResult,
            CancellationToken cancellationToken)
    {
        if (nluResult.Intent is
            IntentType.RegisterPayment or
            IntentType.RegisterCheck)
        {
            if (nluResult.Direction == DirectionHint.Given)
            {
                return new ConversationProcessingResult(
                    ProcessingStatus.Failed,
                    ConversationMessages.OnlyReceivedSupported,
                    null);
            }

            if (HasMoreThanOneAmount(nluResult))
            {
                return new ConversationProcessingResult(
                    ProcessingStatus.Failed,
                    ConversationMessages.MultipleAmounts,
                    null);
            }
        }

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
                    ProcessingStatus.Failed,
                    ConversationMessages.UnsupportedRequest,
                    null)
        };
    }

    private static bool IsCancelMessage(
        string message)
    {
        var tokens = PhraseMatcher.Tokenize(message);

        if (tokens.Count == 0 || tokens.Count > 3)
            return false;

        return tokens.Any(token =>
                   token is "لغو" or "انصراف" or "بیخیال" or "کنسل") ||
               PhraseMatcher.ContainsPhrase(tokens, "بی خیال");
    }

    private static bool IsNewCommand(
        NluResult? nluResult)
    {
        return nluResult is not null &&
               nluResult.Intent is
                   IntentType.RegisterPayment or
                   IntentType.RegisterCheck &&
               nluResult.Entities.Any(x => x.Type == EntityType.Money);
    }

    private static bool HasMoreThanOneAmount(
        NluResult nluResult)
    {
        return nluResult.Entities
            .Where(x => x.Type == EntityType.Money)
            .Select(x => ((MoneyAmount)x.Value!).AmountInToman)
            .Distinct()
            .Count() > 1;
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

        var action = new PendingAction(
            IntentType.RegisterPayment,
            customer.RawValue,
            null,
            (moneyEntity?.Value as MoneyAmount)?.AmountInToman,
            MoneyUnit.Toman,
            DateTime.UtcNow);

        if (action.Amount is null)
            return await AskForAmountAsync(
                conversationId, action, cancellationToken);

        return await ResolveAndCompleteAsync(
            conversationId, action, cancellationToken);
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

        var action = new PendingAction(
            IntentType.RegisterCheck,
            customer.RawValue,
            null,
            (moneyEntity?.Value as MoneyAmount)?.AmountInToman,
            MoneyUnit.Toman,
            dateEntity?.Value as DateTime?,
            null,
            DetectRegistration(nluResult.NormalizedText));

        if (action.Amount is null)
            return await AskForAmountAsync(
                conversationId, action, cancellationToken);

        return await ResolveAndCompleteAsync(
            conversationId, action, cancellationToken);
    }

    private async Task<ConversationProcessingResult>
        AskForAmountAsync(
            string conversationId,
            PendingAction action,
            CancellationToken cancellationToken)
    {
        await conversationStateStore.SaveAsync(
            new ConversationState
            {
                ConversationId = conversationId,
                Status = ConversationStatus.WaitingForAmount,
                PendingAction = action
            },
            cancellationToken);

        return new ConversationProcessingResult(
            ProcessingStatus.Failed,
            AmountRequiredMessage(action),
            null);
    }

    private static string AmountRequiredMessage(PendingAction? action) =>
        action?.Intent == IntentType.RegisterCheck
            ? ConversationMessages.CheckAmountRequired
            : ConversationMessages.PaymentAmountRequired;

    /// <summary>
    /// Shared path: resolve customer (ambiguous -> selection,
    /// not found -> creation confirmation, found -> complete).
    /// </summary>
    private async Task<ConversationProcessingResult>
        ResolveAndCompleteAsync(
            string conversationId,
            PendingAction action,
            CancellationToken cancellationToken)
    {
        var resolution =
            await customerService.ResolveByNameAsync(
                action.CustomerName ?? string.Empty,
                cancellationToken);

        if (resolution.Status ==
            CustomerResolutionStatus.Ambiguous)
        {
            await SaveCustomerSelectionStateAsync(
                conversationId,
                action,
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
            await SaveCustomerCreationConfirmationStateAsync(
                conversationId,
                action,
                cancellationToken);

            return new ConversationProcessingResult(
                ProcessingStatus.Failed,
                ConversationMessages.CustomerNotFound(
                    action.CustomerName ?? string.Empty),
                null);
        }

        if (resolution.Customer is null)
        {
            await conversationStateStore.RemoveAsync(
                conversationId,
                cancellationToken);

            return new ConversationProcessingResult(
                ProcessingStatus.Failed,
                ConversationMessages.CustomerInformationInvalid,
                null);
        }

        return await CompletePendingActionAsync(
            new ConversationState { ConversationId = conversationId },
            action with
            {
                CustomerId = resolution.Customer.CustomerId,
                CustomerName = resolution.Customer.DisplayName
            },
            cancellationToken);
    }

    private async Task<ConversationProcessingResult>
        ProcessBalanceAsync(
            NluResult nluResult,
            CancellationToken cancellationToken)
    {
        var customerEntity =
            nluResult.Entities.FirstOrDefault(
                x => x.Type == EntityType.Customer);

        if (customerEntity?.Value is not CustomerMention customer)
        {
            return new ConversationProcessingResult(
                ProcessingStatus.Failed,
                ConversationMessages.CustomerNameRequired,
                null);
        }

        var resolution =
            await customerService.ResolveByNameAsync(
                customer.RawValue,
                cancellationToken);

        if (resolution.Status == CustomerResolutionStatus.Ambiguous)
        {
            return new ConversationProcessingResult(
                ProcessingStatus.Failed,
                ConversationMessages.BalanceCustomerAmbiguous,
                resolution.Candidates);
        }

        if (resolution.Status == CustomerResolutionStatus.NotFound ||
            resolution.Customer is null)
        {
            return new ConversationProcessingResult(
                ProcessingStatus.Failed,
                ConversationMessages.BalanceCustomerNotFound(
                    customer.RawValue),
                null);
        }

        var summary =
            await ledgerCustomerService.GetAsync(
                resolution.Customer.CustomerId,
                cancellationToken);

        return new ConversationProcessingResult(
            ProcessingStatus.Success,
            ConversationMessages.CustomerBalance(
                resolution.Customer.DisplayName,
                summary.CashReceived,
                summary.ChecksPendingTotal,
                summary.TotalReceived),
            null);
    }

    private static BankRegistrationStatus? DetectRegistration(
        string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        var t = text.Replace('\u200c', ' ');

        if (t.Contains("ثبت نشده") ||
            t.Contains("صیادی نیست") ||
            t.Contains("غیر صیادی"))
            return BankRegistrationStatus.NotRegistered;

        if (t.Contains("صیادی") ||
            t.Contains("ثبت شده"))
            return BankRegistrationStatus.Registered;

        return null;
    }

    private async Task<ConversationProcessingResult>
        HandleAmountAsync(
            ConversationState state,
            string message,
            CancellationToken cancellationToken)
    {
        var action = state.PendingAction;

        if (action is null)
        {
            await conversationStateStore.RemoveAsync(
                state.ConversationId,
                cancellationToken);

            return new ConversationProcessingResult(
                ProcessingStatus.Failed,
                ConversationMessages.PreviousActionNotFound,
                null);
        }

        var reply = nluPipeline.Analyze(message);

        var amounts = reply.Entities
            .Where(x => x.Type == EntityType.Money)
            .Select(x => ((MoneyAmount)x.Value!).AmountInToman)
            .Distinct()
            .ToArray();

        if (amounts.Length == 0)
        {
            return new ConversationProcessingResult(
                ProcessingStatus.Failed,
                AmountRequiredMessage(action),
                null);
        }

        if (amounts.Length > 1)
        {
            return new ConversationProcessingResult(
                ProcessingStatus.Failed,
                ConversationMessages.MultipleAmounts,
                null);
        }

        var updated = action with { Amount = amounts[0] };

        if (action.Intent == IntentType.RegisterCheck)
        {
            var dateEntity = reply.Entities.FirstOrDefault(
                x => x.Type == EntityType.Date);

            updated = updated with
            {
                Date = action.Date ?? dateEntity?.Value as DateTime?,
                Registration = action.Registration ??
                               DetectRegistration(reply.NormalizedText)
            };
        }

        return await ResolveAndCompleteAsync(
            state.ConversationId,
            updated,
            cancellationToken);
    }

    private async Task<ConversationProcessingResult>
        HandleCheckRegistrationAsync(
            ConversationState state,
            string message,
            CancellationToken cancellationToken)
    {
        var action = state.PendingAction;

        if (action is null)
        {
            await conversationStateStore.RemoveAsync(
                state.ConversationId,
                cancellationToken);

            return new ConversationProcessingResult(
                ProcessingStatus.Failed,
                ConversationMessages.CheckInformationNotFound,
                null);
        }

        BankRegistrationStatus? answer =
            DetectRegistration(message) ??
            (IsYes(message) ? BankRegistrationStatus.Registered :
             IsNo(message) ? BankRegistrationStatus.NotRegistered :
             null);

        if (answer is null)
        {
            return new ConversationProcessingResult(
                ProcessingStatus.Failed,
                ConversationMessages.CheckRegistrationQuestion,
                null);
        }

        return await CompletePendingActionAsync(
            state,
            action with { Registration = answer },
            cancellationToken);
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

            ConversationStatus.WaitingForAmount =>
                await HandleAmountAsync(
                    state,
                    message,
                    cancellationToken),

            ConversationStatus.WaitingForCheckRegistration =>
                await HandleCheckRegistrationAsync(
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

        return await CompletePendingActionAsync(
            state,
            action with { Date = date.Value },
            cancellationToken);
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

            if (pendingAction.Registration is null)
            {
                state.PendingAction = pendingAction;

                state.Status =
                    ConversationStatus.WaitingForCheckRegistration;

                await conversationStateStore.SaveAsync(
                    state,
                    cancellationToken);

                return new ConversationProcessingResult(
                    ProcessingStatus.Failed,
                    ConversationMessages.CheckRegistrationQuestion,
                    null);
            }

            await checkService.RegisterAsync(
                new RegisterCheckDto(
                    pendingAction.CustomerId.Value,
                    pendingAction.Amount.Value,
                    pendingAction.Date.Value,
                    pendingAction.CheckNumber,
                    pendingAction.Registration),
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
