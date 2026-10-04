using Vexora.Application.Conversations.Interfaces;
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

namespace Vexora.Application.Conversations.Implementations;

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
        if (nluResult.Intent == IntentType.Unknown)
        {
            return new ConversationProcessingResult(
                ProcessingStatus.Failed,
                "متوجه درخواست شما نشدم.",
                null);
        }

        if (nluResult.Intent == IntentType.RegisterPayment)
        {
            return await ProcessRegisterPaymentAsync(
                conversationId,
                nluResult,
                cancellationToken);
        }

        if (nluResult.Intent == IntentType.RegisterCheck)
        {
            return await ProcessRegisterCheckAsync(
                conversationId,
                nluResult,
                cancellationToken);
        }

        return new ConversationProcessingResult(
            ProcessingStatus.Success,
            "درخواست شناسایی شد.",
            nluResult);
    }

    private async Task<ConversationProcessingResult>
    ProcessRegisterCheckAsync(
        string conversationId,
        NluResult nluResult,
        CancellationToken cancellationToken)
    {
        var customerEntity =
            nluResult.Entities
                .FirstOrDefault(x =>
                    x.Type == EntityType.Customer);

        var moneyEntity =
            nluResult.Entities
                .FirstOrDefault(x =>
                    x.Type == EntityType.Money);

        var dateEntity =
            nluResult.Entities
                .FirstOrDefault(x =>
                    x.Type == EntityType.Date);

        if (customerEntity?.Value is not CustomerMention customer)
        {
            return new ConversationProcessingResult(
                ProcessingStatus.Failed,
                "لطفاً نام مشتری را مشخص کنید.",
                null);
        }

        if (moneyEntity?.Value is not MoneyAmount money)
        {
            return new ConversationProcessingResult(
                ProcessingStatus.Failed,
                "مبلغ چک را مشخص کنید.",
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

            var state = new ConversationState
            {
                ConversationId = conversationId,
                Status =
                    ConversationStatus.WaitingForCustomerSelection,
                PendingAction = pendingAction,
                CustomerCandidates = resolution.Candidates
            };

            await conversationStateStore.SaveAsync(
                state,
                cancellationToken);

            return new ConversationProcessingResult(
                ProcessingStatus.Failed,
                "لطفاً شماره یکی از گزینه‌های زیر را وارد کنید:",
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

            var state = new ConversationState
            {
                ConversationId = conversationId,
                Status =
                    ConversationStatus.WaitingForCustomerCreationConfirmation,
                PendingAction = pendingAction,
                CustomerCandidates = []
            };

            await conversationStateStore.SaveAsync(
                state,
                cancellationToken);

            return new ConversationProcessingResult(
                ProcessingStatus.Failed,
                $"مشتری «{customer.RawValue}» پیدا نشد. آیا می‌خواهید او را ثبت کنید؟",
                null);
        }

        if (resolution.Customer is null)
        {
            return new ConversationProcessingResult(
                ProcessingStatus.Failed,
                "اطلاعات مشتری معتبر نیست.",
                null);
        }

        if (dateEntity?.Value is not DateTime date)
        {
            await conversationStateStore.SaveAsync(
                new ConversationState
                {
                    ConversationId = conversationId,
                    Status =
                        ConversationStatus.WaitingForCheckDate,
                    PendingAction = new PendingAction(
                        IntentType.RegisterCheck,
                        customer.RawValue,
                        resolution.Customer.CustomerId,
                        money.Amount,
                        money.Unit,
                        null)
                },
                cancellationToken);

            return new ConversationProcessingResult(
                ProcessingStatus.Failed,
                "تاریخ سررسید چک را وارد کنید.",
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
            $"چک {money.Amount} برای {resolution.Customer.DisplayName} با موفقیت ثبت شد.",
            null);
    }

    private async Task<ConversationProcessingResult>
    HandleRegisterCheckAsync(
        ConversationState? state,
        NluResult nluResult,
        CancellationToken cancellationToken)
    {
        var customerEntity =
            nluResult.Entities
                .FirstOrDefault(x =>
                    x.Type == EntityType.Customer);


        if (customerEntity is null)
        {
            return new ConversationProcessingResult(
                ProcessingStatus.Failed,
                "لطفاً نام مشتری را مشخص کنید.",
                null);
        }


        var moneyEntity =
            nluResult.Entities
                .FirstOrDefault(x =>
                    x.Type == EntityType.Money);


        if (moneyEntity is null)
        {
            return new ConversationProcessingResult(
                ProcessingStatus.Failed,
                "مبلغ چک مشخص نشده است.",
                null);
        }


        var dateEntity =
            nluResult.Entities
                .FirstOrDefault(x =>
                    x.Type == EntityType.Date);


        if (dateEntity is null)
        {
            return new ConversationProcessingResult(
                ProcessingStatus.Failed,
                "تاریخ سررسید چک را مشخص کنید.",
                null);
        }


        var customerName =
            customerEntity.RawValue;


        return new ConversationProcessingResult(
            ProcessingStatus.Success,
            $"چک {moneyEntity.RawValue} برای {customerName} در تاریخ {dateEntity.RawValue} ثبت خواهد شد.",
            new PendingAction(
                IntentType.RegisterCheck,
                customerName,
                null,
                ((MoneyAmount)moneyEntity.Value!).Amount,
                ((MoneyAmount)moneyEntity.Value!).Unit,
                (DateTime)dateEntity.Value!
            ));
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
                "لطفاً نام مشتری را مشخص کنید.",
                null);
        }

        if (moneyEntity?.Value is not MoneyAmount money)
        {
            return new ConversationProcessingResult(
                ProcessingStatus.Failed,
                "مبلغ پرداخت را مشخص کنید.",
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

            var state = new ConversationState
            {
                ConversationId = conversationId,
                Status =
                    ConversationStatus.WaitingForCustomerSelection,
                PendingAction = pendingAction,
                CustomerCandidates = resolution.Candidates
            };

            await conversationStateStore.SaveAsync(
                state,
                cancellationToken);

            return new ConversationProcessingResult(
                ProcessingStatus.Failed,
                "لطفاً شماره یکی از گزینه‌های زیر را وارد کنید:",
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

            var state = new ConversationState
            {
                ConversationId = conversationId,
                Status =
                    ConversationStatus.WaitingForCustomerCreationConfirmation,
                PendingAction = pendingAction,
                CustomerCandidates = []
            };

            await conversationStateStore.SaveAsync(
                state,
                cancellationToken);

            return new ConversationProcessingResult(
                ProcessingStatus.Failed,
                $"مشتری «{customer.RawValue}» پیدا نشد. آیا می‌خواهید او را ثبت کنید؟",
                null);
        }

        if (resolution.Customer is null)
        {
            return new ConversationProcessingResult(
                ProcessingStatus.Failed,
                "اطلاعات مشتری معتبر نیست.",
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
            $"دریافت مبلغ {money.Amount:N0} تومان از {resolution.Customer.DisplayName} ثبت شد.",
            null);
    }

    private async Task<ConversationProcessingResult>
    ContinueConversationAsync(
        ConversationState state,
        string message,
        CancellationToken cancellationToken)
    {
        if (state.Status ==
            ConversationStatus.WaitingForCheckDate)
        {
            return await HandleCheckDateAsync(
                state,
                message,
                cancellationToken);
        }

        if (state.Status ==
            ConversationStatus.WaitingForCustomerCreationConfirmation)
        {
            return await HandleCustomerCreationConfirmationAsync(
                state,
                message,
                cancellationToken);
        }

        if (state.Status ==
            ConversationStatus.WaitingForCustomerSelection)
        {
            return await HandleCustomerSelectionAsync(
                state,
                message,
                cancellationToken);
        }

        if (state.Status ==
            ConversationStatus.WaitingForCustomerName)
        {
            return await HandleNewCustomerNameAsync(
                state,
                message,
                cancellationToken);
        }

        return new ConversationProcessingResult(
            ProcessingStatus.Failed,
            "ادامه مکالمه مشخص نیست.",
            null);
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
                "تاریخ معتبر وارد کنید.",
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
                "اطلاعات چک در مکالمه پیدا نشد.",
                null);
        }

        await checkService.RegisterAsync(
            new RegisterCheckDto(
                action.CustomerId.Value,
                action.Amount.Value,
                date.Value,
                null),
            cancellationToken);

        await conversationStateStore.RemoveAsync(
            state.ConversationId,
            cancellationToken);

        return new ConversationProcessingResult(
            ProcessingStatus.Success,
            "چک با موفقیت ثبت شد.",
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
                "لطفاً نام مشتری را وارد کنید.",
                null);
        }

        var createCustomerDto =
            BuildCreateCustomerDto(message);

        var customer =
            await customerService.CreateAsync(
                createCustomerDto,
                cancellationToken);

        var pendingAction = state.PendingAction;

        if (pendingAction is not null &&
            pendingAction.Intent == IntentType.RegisterPayment)
        {
            if (!pendingAction.Amount.HasValue)
            {
                await conversationStateStore.RemoveAsync(
                    state.ConversationId,
                    cancellationToken);

                return new ConversationProcessingResult(
                    ProcessingStatus.Failed,
                    "مبلغ پرداخت در مکالمه پیدا نشد.",
                    null);
            }

            await paymentService.RegisterAsync(
                new RegisterPaymentDto(
                    customer.CustomerId,
                    pendingAction.Amount.Value,
                    pendingAction.Date),
                cancellationToken);
        }

        var displayName =
            $"{customer.FirstName} {customer.LastName}".Trim();

        await conversationStateStore.RemoveAsync(
            state.ConversationId,
            cancellationToken);

        return new ConversationProcessingResult(
            ProcessingStatus.Success,
            $"مشتری «{displayName}» ثبت شد و عملیات با موفقیت انجام شد.",
            null);
    }

    private async Task<ConversationProcessingResult>
     HandleCustomerSelectionAsync(
         ConversationState state,
         string message,
         CancellationToken cancellationToken)
    {
        if (!int.TryParse(message.Trim(), out var selection))
        {
            return new ConversationProcessingResult(
                ProcessingStatus.Failed,
                "لطفاً شماره یکی از گزینه‌های زیر را وارد کنید.",
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
                "لطفاً نام و در صورت تمایل نام خانوادگی مشتری جدید را وارد کنید.",
                null);
        }

        if (selection < 1 ||
            selection > candidates.Length)
        {
            return new ConversationProcessingResult(
                ProcessingStatus.Failed,
                "شماره واردشده معتبر نیست. لطفاً شماره یکی از گزینه‌ها را وارد کنید.",
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
                "اطلاعات عملیات قبلی پیدا نشد.",
                null);
        }

        if (pendingAction.Intent ==
            IntentType.RegisterPayment)
        {
            if (!pendingAction.Amount.HasValue)
            {
                await conversationStateStore.RemoveAsync(
                    state.ConversationId,
                    cancellationToken);

                return new ConversationProcessingResult(
                    ProcessingStatus.Failed,
                    "مبلغ پرداخت در مکالمه پیدا نشد.",
                    null);
            }

            await paymentService.RegisterAsync(
                new RegisterPaymentDto(
                    selectedCustomer.CustomerId,
                    pendingAction.Amount.Value,
                    pendingAction.Date),
                cancellationToken);

            await conversationStateStore.RemoveAsync(
                state.ConversationId,
                cancellationToken);

            return new ConversationProcessingResult(
                ProcessingStatus.Success,
                $"دریافت مبلغ {pendingAction.Amount.Value:N0} تومان از {selectedCustomer.DisplayName} ثبت شد.",
                null);
        }

        if (pendingAction.Intent ==
            IntentType.RegisterCheck)
        {
            if (!pendingAction.Amount.HasValue)
            {
                await conversationStateStore.RemoveAsync(
                    state.ConversationId,
                    cancellationToken);

                return new ConversationProcessingResult(
                    ProcessingStatus.Failed,
                    "مبلغ چک در مکالمه پیدا نشد.",
                    null);
            }

            if (!pendingAction.Date.HasValue)
            {
                state.PendingAction =
                    pendingAction with
                    {
                        CustomerId =
                            selectedCustomer.CustomerId
                    };

                state.Status =
                    ConversationStatus.WaitingForCheckDate;

                await conversationStateStore.SaveAsync(
                    state,
                    cancellationToken);

                return new ConversationProcessingResult(
                    ProcessingStatus.Failed,
                    "تاریخ سررسید چک را وارد کنید.",
                    null);
            }

            await checkService.RegisterAsync(
                new RegisterCheckDto(
                    selectedCustomer.CustomerId,
                    pendingAction.Amount.Value,
                    pendingAction.Date.Value,
                    null),
                cancellationToken);

            await conversationStateStore.RemoveAsync(
                state.ConversationId,
                cancellationToken);

            return new ConversationProcessingResult(
                ProcessingStatus.Success,
                $"چک {pendingAction.Amount.Value:N0} تومانی برای «{selectedCustomer.DisplayName}» با موفقیت ثبت شد.",
                null);
        }

        return new ConversationProcessingResult(
            ProcessingStatus.Failed,
            "نوع درخواست قبلی پشتیبانی نمی‌شود.",
            null);
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
                "لطفاً پاسخ دهید که آیا مشتری ثبت شود یا خیر.",
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
                "ثبت مشتری لغو شد.",
                null);
        }

        return new ConversationProcessingResult(
            ProcessingStatus.Failed,
            "لطفاً با «بله» یا «خیر» پاسخ دهید.",
            null);
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

    private async Task<ConversationProcessingResult>
        CreateCustomerAndCompleteActionAsync(
            ConversationState state,
            CancellationToken cancellationToken)
    {
        var pendingAction = state.PendingAction;

        if (pendingAction is null ||
            string.IsNullOrWhiteSpace(pendingAction.CustomerName))
        {
            await conversationStateStore.RemoveAsync(
                state.ConversationId,
                cancellationToken);

            return new ConversationProcessingResult(
                ProcessingStatus.Failed,
                "اطلاعات مشتری در مکالمه پیدا نشد.",
                null);
        }

        var createCustomerDto =
            BuildCreateCustomerDto(
                pendingAction.CustomerName);

        var customer =
            await customerService.CreateAsync(
                createCustomerDto,
                cancellationToken);

        if (pendingAction.Intent == IntentType.RegisterPayment)
        {
            if (!pendingAction.Amount.HasValue)
            {
                await conversationStateStore.RemoveAsync(
                    state.ConversationId,
                    cancellationToken);

                return new ConversationProcessingResult(
                    ProcessingStatus.Failed,
                    "مبلغ پرداخت در مکالمه پیدا نشد.",
                    null);
            }

            await paymentService.RegisterAsync(
                new RegisterPaymentDto(
                    customer.CustomerId,
                    pendingAction.Amount.Value,
                    pendingAction.Date),
                cancellationToken);
        }
        else if (pendingAction.Intent ==
         IntentType.RegisterCheck)
        {
            if (!pendingAction.Amount.HasValue)
            {
                await conversationStateStore.RemoveAsync(
                    state.ConversationId,
                    cancellationToken);

                return new ConversationProcessingResult(
                    ProcessingStatus.Failed,
                    "مبلغ چک در مکالمه پیدا نشد.",
                    null);
            }

            if (!pendingAction.Date.HasValue)
            {
                state.PendingAction =
                    pendingAction with
                    {
                        CustomerId = customer.CustomerId
                    };

                state.Status =
                    ConversationStatus.WaitingForCheckDate;

                await conversationStateStore.SaveAsync(
                    state,
                    cancellationToken);

                return new ConversationProcessingResult(
                    ProcessingStatus.Failed,
                    "تاریخ سررسید چک را وارد کنید.",
                    null);
            }

            await checkService.RegisterAsync(
                new RegisterCheckDto(
                    customer.CustomerId,
                    pendingAction.Amount.Value,
                    pendingAction.Date.Value,
                    null),
                cancellationToken);
        }

        await conversationStateStore.RemoveAsync(
            state.ConversationId,
            cancellationToken);

        var message =
     pendingAction.Intent == IntentType.RegisterCheck
         ? $"مشتری «{pendingAction.CustomerName}» ثبت شد و چک با موفقیت ثبت شد."
         : $"مشتری «{pendingAction.CustomerName}» ثبت شد و پرداخت با موفقیت ثبت شد.";

        return new ConversationProcessingResult(
            ProcessingStatus.Success,
            message,
            null);
    }

    private static CreateCustomerDto BuildCreateCustomerDto(
    string customerName)
    {
        var parts = customerName
            .Trim()
            .Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries);

        return parts.Length switch
        {
            0 => throw new ArgumentException(
                "نام مشتری نمی‌تواند خالی باشد.",
                nameof(customerName)),

            1 => new CreateCustomerDto(
                parts[0],
                string.Empty),

            _ => new CreateCustomerDto(
                parts[0],
                string.Join(' ', parts.Skip(1)))
        };
    }
}