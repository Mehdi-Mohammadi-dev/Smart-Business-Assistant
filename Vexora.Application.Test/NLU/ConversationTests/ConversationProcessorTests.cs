using Moq;
using Vexora.Application.Constants.Messages.Conversations;
using Vexora.Application.Conversations;
using Vexora.Application.Conversations.Implementations;
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
using Xunit;

namespace Vexora.Application.Test.NLU.ConversationTests;

public sealed class ConversationProcessorTests
{
    [Fact]
    public async Task ProcessAsync_WhenCustomerIsAmbiguous_ShouldSaveConversationState()
    {
        // Arrange
        var nluPipeline =
            new Mock<INluPipeline>();

        var customerService =
            new Mock<ICustomerService>();

        var conversationStore =
            new Mock<IConversationStateStore>();

        var paymentService =
            new Mock<IPaymentService>();

        var checkService =
            new Mock<ICheckService>();

        var dateExtractor =
            new Mock<IDateExtractor>();

        var candidates = new[]
        {
            new CustomerCandidateDto(
                1,
                "مهدی محمدی"),

            new CustomerCandidateDto(
                2,
                "مهدی احمدی")
        };

        nluPipeline
            .Setup(x => x.Analyze(
                It.IsAny<string>()))
            .Returns(CreateRegisterPaymentNluResult());

        customerService
            .Setup(x => x.ResolveByNameAsync(
                "مهدی",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new CustomerResolutionResultDto(
                    CustomerResolutionStatus.Ambiguous,
                    null,
                    candidates));

        var service =
            new ConversationProcessor(
                nluPipeline.Object,
                conversationStore.Object,
                customerService.Object,
                paymentService.Object,
                checkService.Object,
                dateExtractor.Object);

        var request = new ChatRequestDto(
            "conversation-1",
            "از مهدی 150 تومن گرفتم");

        // Act
        var result =
            await service.ProcessAsync(request);

        // Assert
        Assert.True(
            result.Status == ProcessingStatus.Failed);

        Assert.Equal(
            "لطفاً شماره یکی از گزینه‌های زیر را وارد کنید:",
            result.Message);

        conversationStore.Verify(
            x => x.SaveAsync(
                It.Is<ConversationState>(state =>
                    state.ConversationId ==
                        "conversation-1" &&

                    state.Status ==
                        ConversationStatus
                            .WaitingForCustomerSelection &&

                    state.PendingAction != null &&

                    state.PendingAction.Intent ==
                        IntentType.RegisterPayment &&

                    state.PendingAction.CustomerName ==
                        "مهدی" &&

                    state.PendingAction.Amount ==
                        150000 &&

                    state.CustomerCandidates.Count ==
                        2),
                It.IsAny<CancellationToken>()),
            Times.Once);

        paymentService.Verify(
            x => x.RegisterAsync(
                It.IsAny<RegisterPaymentDto>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessAsync_WhenCustomerSelectionIsValid_ShouldRegisterPayment()
    {
        // Arrange
        var conversationStore =
            new Mock<IConversationStateStore>();

        var paymentService =
            new Mock<IPaymentService>();

        var customerService =
            new Mock<ICustomerService>();

        var nluPipeline =
            new Mock<INluPipeline>();

        var checkService =
            new Mock<ICheckService>();

        var dateExtractor =
            new Mock<IDateExtractor>();

        var state = new ConversationState
        {
            ConversationId = "conversation-1",

            Status =
                ConversationStatus
                    .WaitingForCustomerSelection,

            PendingAction = new PendingAction(
                IntentType.RegisterPayment,
                "مهدی",
                null,
                150000,
                MoneyUnit.Toman,
                DateTime.Now),

            CustomerCandidates =
            [
                new CustomerCandidateDto(
                    1,
                    "مهدی محمدی"),

                new CustomerCandidateDto(
                    2,
                    "مهدی احمدی")
            ]
        };

        conversationStore
            .Setup(x => x.GetAsync(
                "conversation-1",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(state);

        var service =
            new ConversationProcessor(
                nluPipeline.Object,
                conversationStore.Object,
                customerService.Object,
                paymentService.Object,
                checkService.Object,
                dateExtractor.Object);

        var request = new ChatRequestDto(
            "conversation-1",
            "2");

        // Act
        var result =
            await service.ProcessAsync(request);

        // Assert
        Assert.Equal(
            ProcessingStatus.Success,
            result.Status);

        Assert.False(
            string.IsNullOrWhiteSpace(
                result.Message));

        paymentService.Verify(
            x => x.RegisterAsync(
                It.Is<RegisterPaymentDto>(dto =>
                    dto.CustomerId == 2 &&
                    dto.Amount == 150000),
                It.IsAny<CancellationToken>()),
            Times.Once);

        conversationStore.Verify(
            x => x.RemoveAsync(
                "conversation-1",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessAsync_WhenCustomerSelectionIsInvalid_ShouldAskAgain()
    {
        // Arrange
        var conversationStore =
            new Mock<IConversationStateStore>();

        var paymentService =
            new Mock<IPaymentService>();

        var customerService =
            new Mock<ICustomerService>();

        var nluPipeline =
            new Mock<INluPipeline>();

        var checkService =
              new Mock<ICheckService>();

        var dateExtractor =
            new Mock<IDateExtractor>();

        var state = new ConversationState
        {
            ConversationId = "conversation-1",

            Status =
                ConversationStatus
                    .WaitingForCustomerSelection,

            PendingAction = new PendingAction(
                IntentType.RegisterPayment,
                "مهدی",
                null,
                150000,
                MoneyUnit.Toman,
                DateTime.Now),

            CustomerCandidates =
            [
                new CustomerCandidateDto(
                    1,
                    "مهدی محمدی"),

                new CustomerCandidateDto(
                    2,
                    "مهدی احمدی")
            ]
        };

        conversationStore
            .Setup(x => x.GetAsync(
                "conversation-1",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(state);

        var service =
            new ConversationProcessor(
                nluPipeline.Object,
                conversationStore.Object,
                customerService.Object,
                paymentService.Object,
                checkService.Object,
                dateExtractor.Object);

        // Act
        var result =
            await service.ProcessAsync(
                new ChatRequestDto(
                    "conversation-1",
                    "5"));

        // Assert
        Assert.Equal(
            ProcessingStatus.Failed,
            result.Status);

        Assert.Contains(
            "معتبر",
            result.Message);

        paymentService.Verify(
            x => x.RegisterAsync(
                It.IsAny<RegisterPaymentDto>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        conversationStore.Verify(
            x => x.RemoveAsync(
                "conversation-1",
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessAsync_WhenCustomerNotFound_ShouldWaitForCreationConfirmation()
    {
        // Arrange
        var nluPipeline =
            new Mock<INluPipeline>();

        var customerService =
            new Mock<ICustomerService>();

        var conversationStore =
            new Mock<IConversationStateStore>();

        var paymentService =
            new Mock<IPaymentService>();

        var checkService =
            new Mock<ICheckService>();

        var dateExtractor =
    new Mock<IDateExtractor>();

        ConversationState? savedState = null;

        nluPipeline
            .Setup(x => x.Analyze(
                It.IsAny<string>()))
            .Returns(CreateRegisterPaymentNluResult());

        customerService
            .Setup(x => x.ResolveByNameAsync(
                "مهدی",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new CustomerResolutionResultDto(
                    CustomerResolutionStatus.NotFound,
                    null,
                    []));

        conversationStore
            .Setup(x => x.SaveAsync(
                It.IsAny<ConversationState>(),
                It.IsAny<CancellationToken>()))
            .Callback<ConversationState, CancellationToken>(
                (state, _) => savedState = state);

        var service =
            new ConversationProcessor(
                nluPipeline.Object,
                conversationStore.Object,
                customerService.Object,
                paymentService.Object,
                checkService.Object,
                dateExtractor.Object);

        // Act
        var result =
            await service.ProcessAsync(
                new ChatRequestDto(
                    "conversation-1",
                    "از مهدی 150 تومن گرفتم"));

        // Assert
        Assert.Equal(
            ProcessingStatus.Failed,
            result.Status);

        Assert.True(
            result.Message!.Contains("ثبت"));

        Assert.NotNull(savedState);

        Assert.Equal(
            ConversationStatus
                .WaitingForCustomerCreationConfirmation,
            savedState!.Status);

        Assert.NotNull(
            savedState.PendingAction);

        Assert.Equal(
            "مهدی",
            savedState.PendingAction!.CustomerName);

        Assert.Equal(
            150000,
            savedState.PendingAction.Amount);

        conversationStore.Verify(
            x => x.SaveAsync(
                It.IsAny<ConversationState>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        paymentService.Verify(
            x => x.RegisterAsync(
                It.IsAny<RegisterPaymentDto>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static NluResult CreateRegisterPaymentNluResult()
    {
        return new NluResult(
            "از مهدی 150 تومن گرفتم",
            "از مهدی 150 تومان گرفتم",
            IntentType.RegisterPayment,
            0.95,

            [],

            [
                new EntityResult(
                    EntityType.Customer,
                    "مهدی",
                    new CustomerMention(
                        "مهدی",
                        3,
                        4,
                        0.85),
                    3,
                    4,
                    0.85),

                new EntityResult(
                    EntityType.Money,
                    "150 تومان",
                    new MoneyAmount(
                        150000,
                        MoneyUnit.Toman),
                    8,
                    9,
                    0.95)
            ]);
    }

    [Fact]
    public async Task ProcessAsync_WhenCustomerIsFound_ShouldRegisterPayment()
    {
        // Arrange
        var nluPipeline = new Mock<INluPipeline>();
        var customerService = new Mock<ICustomerService>();
        var conversationStore = new Mock<IConversationStateStore>();
        var paymentService = new Mock<IPaymentService>();
        var checkService = new Mock<ICheckService>();
        var dateExtractor = new Mock<IDateExtractor>();

        nluPipeline
            .Setup(x => x.Analyze(It.IsAny<string>()))
            .Returns(CreateRegisterPaymentNluResult());

        customerService
            .Setup(x => x.ResolveByNameAsync(
                "مهدی",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new CustomerResolutionResultDto(
                    CustomerResolutionStatus.Resolved,
                    new CustomerCandidateDto(
                        1,
                        "مهدی محمدی"),
                    []));

        var service = new ConversationProcessor(
            nluPipeline.Object,
            conversationStore.Object,
            customerService.Object,
            paymentService.Object,
            checkService.Object,
            dateExtractor.Object);

        // Act
        var result = await service.ProcessAsync(
            new ChatRequestDto(
                "conversation-1",
                "از مهدی 150 تومن گرفتم"));

        // Assert
        Assert.Equal(
            ProcessingStatus.Success,
            result.Status);

        paymentService.Verify(
            x => x.RegisterAsync(
                It.Is<RegisterPaymentDto>(dto =>
                    dto.CustomerId == 1 &&
                    dto.Amount == 150000),
                It.IsAny<CancellationToken>()),
            Times.Once);

        conversationStore.Verify(
            x => x.SaveAsync(
                It.IsAny<ConversationState>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessAsync_WhenCustomerNotFoundAndUserConfirms_ShouldCreateCustomerAndRegisterPayment()
    {
        // Arrange
        var nluPipeline = new Mock<INluPipeline>();
        var customerService = new Mock<ICustomerService>();
        var conversationStore = new Mock<IConversationStateStore>();
        var paymentService = new Mock<IPaymentService>();
        var checkService = new Mock<ICheckService>();
        var dateExtractor = new Mock<IDateExtractor>();

        var state = new ConversationState
        {
            ConversationId = "conversation-1",

            Status =
                ConversationStatus
                    .WaitingForCustomerCreationConfirmation,

            PendingAction = new PendingAction(
                IntentType.RegisterPayment,
                "علی",
                null,
                100000,
                MoneyUnit.Toman,
                DateTime.Now)
        };

        conversationStore
            .Setup(x => x.GetAsync(
                "conversation-1",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(state);

        customerService
            .Setup(x => x.CreateAsync(
                It.IsAny<CreateCustomerDto>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new CustomerDto(
                    10,
                    "علی",
                    ""));

        var service = new ConversationProcessor(
            nluPipeline.Object,
            conversationStore.Object,
            customerService.Object,
            paymentService.Object,
            checkService.Object,
            dateExtractor.Object);

        // Act
        var result = await service.ProcessAsync(
            new ChatRequestDto(
                "conversation-1",
                "بله"));

        // Assert
        Assert.Equal(
            ProcessingStatus.Success,
            result.Status);

        customerService.Verify(
            x => x.CreateAsync(
                It.Is<CreateCustomerDto>(dto =>
                    dto.Name == "علی"),
                It.IsAny<CancellationToken>()),
            Times.Once);

        paymentService.Verify(
            x => x.RegisterAsync(
                It.Is<RegisterPaymentDto>(dto =>
                    dto.CustomerId == 10 &&
                    dto.Amount == 100000),
                It.IsAny<CancellationToken>()),
            Times.Once);

        conversationStore.Verify(
            x => x.RemoveAsync(
                "conversation-1",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessAsync_WhenCustomerIsFoundAndCheckHasDate_ShouldRegisterCheck()
    {
        // Arrange
        var nluPipeline = new Mock<INluPipeline>();
        var customerService = new Mock<ICustomerService>();
        var conversationStore = new Mock<IConversationStateStore>();
        var paymentService = new Mock<IPaymentService>();
        var checkService = new Mock<ICheckService>();
        var dateExtractor = new Mock<IDateExtractor>();

        var dueDate = new DateTime(2026, 10, 10);

        nluPipeline
            .Setup(x => x.Analyze(It.IsAny<string>()))
            .Returns(CreateRegisterCheckNluResult(dueDate));

        customerService
            .Setup(x => x.ResolveByNameAsync(
                "علی",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new CustomerResolutionResultDto(
                    CustomerResolutionStatus.Resolved,
                    new CustomerCandidateDto(
                        5,
                        "علی کریمی"),
                    []));

        var service = new ConversationProcessor(
            nluPipeline.Object,
            conversationStore.Object,
            customerService.Object,
            paymentService.Object,
            checkService.Object,
            dateExtractor.Object);

        // Act
        var result = await service.ProcessAsync(
            new ChatRequestDto(
                "conversation-check-1",
                "از علی یک چک 50 میلیون تومنی برای 10 مهر گرفتم"));

        // Assert
        Assert.Equal(
            ProcessingStatus.Success,
            result.Status);

        checkService.Verify(
            x => x.RegisterAsync(
                It.Is<RegisterCheckDto>(dto =>
                    dto.CustomerId == 5 &&
                    dto.Amount == 50000000 &&
                    dto.DueDate == dueDate),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static NluResult CreateRegisterCheckNluResult(
    DateTime dueDate)
    {
        return new NluResult(
            "از علی یک چک 50 میلیون تومنی برای 10 مهر گرفتم",
            "از علی یک چک 50 میلیون تومنی برای 10 مهر گرفتم",
            IntentType.RegisterCheck,
            0.95,
            [],
            [
                new EntityResult(
                EntityType.Customer,
                "علی",
                new CustomerMention(
                    "علی",
                    3,
                    4,
                    0.90),
                3,
                4,
                0.90),

            new EntityResult(
                EntityType.Money,
                "50 میلیون تومان",
                new MoneyAmount(
                    50000000,
                    MoneyUnit.Toman),
                10,
                20,
                0.95),

            new EntityResult(
                EntityType.Date,
                "10 مهر",
                dueDate,
                25,
                31,
                0.90)
            ]);
    }

    [Fact]
    public async Task ProcessAsync_WhenCheckDateIsMissing_ShouldWaitForCheckDate()
    {
        // Arrange
        var nluPipeline = new Mock<INluPipeline>();
        var customerService = new Mock<ICustomerService>();
        var conversationStore = new Mock<IConversationStateStore>();
        var paymentService = new Mock<IPaymentService>();
        var checkService = new Mock<ICheckService>();
        var dateExtractor = new Mock<IDateExtractor>();

        nluPipeline
            .Setup(x => x.Analyze(It.IsAny<string>()))
            .Returns(CreateRegisterCheckWithoutDateNluResult());

        customerService
            .Setup(x => x.ResolveByNameAsync(
                "علی",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new CustomerResolutionResultDto(
                    CustomerResolutionStatus.Resolved,
                    new CustomerCandidateDto(
                        5,
                        "علی کریمی"),
                    []));

        ConversationState? savedState = null;

        conversationStore
            .Setup(x => x.SaveAsync(
                It.IsAny<ConversationState>(),
                It.IsAny<CancellationToken>()))
            .Callback<ConversationState, CancellationToken>(
                (state, _) => savedState = state);

        var service = new ConversationProcessor(
            nluPipeline.Object,
            conversationStore.Object,
            customerService.Object,
            paymentService.Object,
            checkService.Object,
            dateExtractor.Object);

        // Act
        var result = await service.ProcessAsync(
            new ChatRequestDto(
                "conversation-check-1",
                "از علی یک چک 50 میلیون تومنی گرفتم"));

        // Assert
        Assert.Equal(
            ProcessingStatus.Failed,
            result.Status);

        Assert.Equal(
            ConversationMessages.CheckDueDateRequired,
            result.Message);

        Assert.NotNull(savedState);

        Assert.Equal(
            ConversationStatus.WaitingForCheckDate,
            savedState!.Status);

        Assert.Equal(
            5,
            savedState.PendingAction!.CustomerId);

        Assert.Equal(
            50000000,
            savedState.PendingAction.Amount);

        Assert.Equal(
            "علی کریمی",
            savedState.PendingAction.CustomerName);

        checkService.Verify(
            x => x.RegisterAsync(
                It.IsAny<RegisterCheckDto>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static NluResult CreateRegisterCheckWithoutDateNluResult()
    {
        return new NluResult(
            "از علی یک چک 50 میلیون تومنی گرفتم",
            "از علی یک چک 50 میلیون تومنی گرفتم",
            IntentType.RegisterCheck,
            0.95,
            [],
            [
                new EntityResult(
                EntityType.Customer,
                "علی",
                new CustomerMention(
                    "علی",
                    3,
                    4,
                    0.90),
                3,
                4,
                0.90),

            new EntityResult(
                EntityType.Money,
                "50 میلیون تومان",
                new MoneyAmount(
                    50000000,
                    MoneyUnit.Toman),
                10,
                20,
                0.95)
            ]);
    }

    [Fact]
    public async Task ProcessAsync_WhenWaitingForCheckDateAndDateIsProvided_ShouldRegisterCheck()
    {
        // Arrange
        var conversationStore =
            new Mock<IConversationStateStore>();

        var paymentService =
            new Mock<IPaymentService>();

        var customerService =
            new Mock<ICustomerService>();

        var nluPipeline =
            new Mock<INluPipeline>();

        var checkService =
            new Mock<ICheckService>();

        var dateExtractor =
            new Mock<IDateExtractor>();

        var dueDate =
            new DateTime(2026, 10, 10);

        var state = new ConversationState
        {
            ConversationId = "conversation-check-1",

            Status =
                ConversationStatus.WaitingForCheckDate,

            PendingAction = new PendingAction(
                IntentType.RegisterCheck,
                "علی کریمی",
                5,
                50000000,
                MoneyUnit.Toman,
                null)
        };

        conversationStore
            .Setup(x => x.GetAsync(
                "conversation-check-1",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(state);

        dateExtractor
            .Setup(x => x.Extract("10 مهر"))
            .Returns(dueDate);

        var service = new ConversationProcessor(
            nluPipeline.Object,
            conversationStore.Object,
            customerService.Object,
            paymentService.Object,
            checkService.Object,
            dateExtractor.Object);

        // Act
        var result = await service.ProcessAsync(
            new ChatRequestDto(
                "conversation-check-1",
                "10 مهر"));

        // Assert
        Assert.Equal(
            ProcessingStatus.Success,
            result.Status);

        checkService.Verify(
            x => x.RegisterAsync(
                It.Is<RegisterCheckDto>(dto =>
                    dto.CustomerId == 5 &&
                    dto.Amount == 50000000 &&
                    dto.DueDate == dueDate),
                It.IsAny<CancellationToken>()),
            Times.Once);

        conversationStore.Verify(
            x => x.RemoveAsync(
                "conversation-check-1",
                It.IsAny<CancellationToken>()),
            Times.Once);

        Assert.Contains(
            "علی کریمی",
            result.Message);
    }

}