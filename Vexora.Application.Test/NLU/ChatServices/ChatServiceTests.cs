using Moq;
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
using Vexora.Application.Services.Implementations.Chats;
using Vexora.Application.Services.Interfaces.Customers;
using Vexora.Application.Services.Interfaces.Payments;
using Xunit;

namespace Voxora.Application.Test.NLU.ChatServices;

public sealed class ChatServiceTests
{
    [Fact]
    public async Task ProcessAsync_WhenCustomerIsAmbiguous_ShouldSaveConversationState()
    {
        // Arrange
        var nluPipeline = new Mock<INluPipeline>();
        var customerService = new Mock<ICustomerService>();
        var conversationStore =
            new Mock<IConversationStateStore>();
        var paymentService = new Mock<IPaymentService>();

        var candidates = new[]
        {
            new CustomerCandidateDto
            (
                1,
                "مهدی محمدی"
            ),
            new CustomerCandidateDto
            (
                2,
                "مهدی احمدی"
            )
        };

        customerService
            .Setup(x => x.ResolveByNameAsync(
                "مهدی",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CustomerResolutionResultDto
            (
                CustomerResolutionStatus.Ambiguous,
                null,
                candidates
            ));

        // این قسمت را با MoneyAmount واقعی پروژه خودت تنظیم کن.
        nluPipeline
            .Setup(x => x.Analyze(It.IsAny<string>()))
            .Returns(CreateRegisterPaymentNluResult());

        var service = new ChatService(
            nluPipeline.Object,
            customerService.Object,
            conversationStore.Object,
            paymentService.Object);

        var request = new ChatRequestDto
        (
            "conversation-1",
            "از مهدی 150 تومن گرفتم"
        );

        // Act
        var result = await service.ProcessAsync(request);

        // Assert
        Assert.True(result.RequiresUserInput);
        Assert.Equal(2, result.Options.Count);

        conversationStore.Verify(
            x => x.SaveAsync(
                It.Is<ConversationState>(state =>
                    state.ConversationId == "conversation-1" &&
                    state.Status ==
                        ConversationStatus.WaitingForCustomerSelection &&
                    state.PendingAction != null &&
                    state.PendingAction.Intent ==
                        IntentType.RegisterPayment &&
                    state.PendingAction.CustomerName == "مهدی" &&
                    state.PendingAction.Amount == 150000 &&
                    state.CustomerCandidates.Count == 2),
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
        return new NluResult
        (
            "از مهدی 150 تومن گرفتم",
            "از مهدی 150 تومان گرفتم",
            IntentType.RegisterPayment,
            0.95,

            // Candidates
            [],

            // Entities
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
            ]
        );
    }

    [Fact]
    public async Task ProcessAsync_WhenCustomerSelectionIsValid_ShouldRegisterPayment()
    {
        // Arrange
        var nluPipeline = new Mock<INluPipeline>();
        var customerService = new Mock<ICustomerService>();
        var conversationStore =
            new Mock<IConversationStateStore>();
        var paymentService = new Mock<IPaymentService>();

        var state = new ConversationState
        {
            ConversationId = "conversation-1",

            Status =
                ConversationStatus.WaitingForCustomerSelection,

            PendingAction = new PendingAction
            (
                IntentType.RegisterPayment,
                "مهدی",
                1,
                150000, MoneyUnit.Toman, DateTime.Now
            ),

            CustomerCandidates =
            [
                new CustomerCandidateDto
            (
                1,
                "مهدی محمدی"
            ),

            new CustomerCandidateDto
            (
                2,
                "مهدی احمدی"
            )
            ]
        };

        conversationStore
            .Setup(x => x.GetAsync(
                "conversation-1",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(state);

        var service = new ChatService(
            nluPipeline.Object,
            customerService.Object,
            conversationStore.Object,
            paymentService.Object);

        var request = new ChatRequestDto
        (
            "conversation-1",
            "2"
        );

        // Act
        var result = await service.ProcessAsync(request);

        // Assert
        Assert.False(result.RequiresUserInput);

        Assert.Contains(
            "مهدی احمدی",
            result.Message);

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
        var state = new ConversationState
        {
            ConversationId = "conversation-1",

            Status =
                ConversationStatus.WaitingForCustomerSelection,

            PendingAction = new PendingAction
            (
                IntentType.RegisterPayment,
                "مهدی",
                1,
                150000,
                MoneyUnit.Rial,
                DateTime.Now
            ),

            CustomerCandidates =
            [
                new CustomerCandidateDto
            (
                1,
                "مهدی محمدی"
            ),

            new CustomerCandidateDto
            (
                2,
                "مهدی احمدی"
            )
            ]
        };

        var store =
            new Mock<IConversationStateStore>();

        store
            .Setup(x => x.GetAsync(
                "conversation-1",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(state);

        var paymentService =
            new Mock<IPaymentService>();

        var service = new ChatService(
            Mock.Of<INluPipeline>(),
            Mock.Of<ICustomerService>(),
            store.Object,
            paymentService.Object);

        // Act
        var result = await service.ProcessAsync(
            new ChatRequestDto
            (
                "conversation-1",
                "5"
            ));

        // Assert
        Assert.True(result.RequiresUserInput);

        Assert.Contains(
            "معتبر",
            result.Message);

        paymentService.Verify(
            x => x.RegisterAsync(
                It.IsAny<RegisterPaymentDto>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessAsync_WhenCustomerNotFound_ShouldWaitForCreationConfirmation()
    {
        // Arrange
        var nluPipeline = new Mock<INluPipeline>();
        var customerService = new Mock<ICustomerService>();
        var store = new Mock<IConversationStateStore>();
        var paymentService = new Mock<IPaymentService>();

        ConversationState? savedState = null;

        nluPipeline
            .Setup(x => x.Analyze(It.IsAny<string>()))
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

        store
            .Setup(x => x.SaveAsync(
                It.IsAny<ConversationState>(),
                It.IsAny<CancellationToken>()))
            .Callback<ConversationState, CancellationToken>(
                (state, _) => savedState = state);

        var service = new ChatService(
            nluPipeline.Object,
            customerService.Object,
            store.Object,
            paymentService.Object);

        // Act
        var result = await service.ProcessAsync(
            new ChatRequestDto(
                "conversation-1",
                "از مهدی 150 تومن گرفتم"));

        // Assert
        Assert.True(result.RequiresUserInput);

        Assert.Equal(2, result.Options.Count);

        Assert.NotNull(savedState);

        Assert.Equal(
            ConversationStatus.WaitingForCustomerCreationConfirmation,
            savedState!.Status);

        Assert.NotNull(savedState.PendingAction);

        Assert.Equal(
            "مهدی",
            savedState.PendingAction!.CustomerName);

        Assert.Equal(
            150000,
            savedState.PendingAction.Amount);

        store.Verify(
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
}