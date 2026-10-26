using Moq;
using Vexora.Application.Caching.Conversations;
using Vexora.Application.Caching.Conversations.Implementations;
using Vexora.Application.Caching.Conversations.Interfaces;
using Vexora.Application.Dtos.Chats;
using Vexora.Application.Dtos.Checks;
using Vexora.Application.Ledger.Customers;
using Vexora.Application.NLU.Abstractions.Dates;
using Vexora.Application.NLU.Abstractions.Pipeline;
using Vexora.Application.NLU.Models.Intents;
using Vexora.Application.NLU.Models.Money;
using Vexora.Application.NLU.Models.Results;
using Vexora.Application.Services.Interfaces.Checks;
using Vexora.Application.Services.Interfaces.Customers;
using Vexora.Application.Services.Interfaces.Payments;
using Vexora.Domain.Enums.BankRegistrationStatuses;
using Xunit;

namespace Vexora.Application.Test.NLU.ConversationTests;

public sealed class ConversationRegistrationAndAmountTests
{
    private static (ConversationProcessor Svc, Mock<ICheckService> Check, Mock<IConversationStateStore> Store)
        Create(ConversationState state, DateTime? date = null)
    {
        var store = new Mock<IConversationStateStore>();
        var check = new Mock<ICheckService>();
        var dates = new Mock<IDateExtractor>();

        store.Setup(x => x.GetAsync(state.ConversationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(state);

        var svc = new ConversationProcessor(
            new Mock<INluPipeline>().Object,
            store.Object,
            new Mock<ICustomerService>().Object,
            new Mock<IPaymentService>().Object,
            check.Object,
            dates.Object,
            new Mock<ILedgerCustomerService>().Object);

        return (svc, check, store);
    }

    private static ConversationState WaitingForRegistration() => new()
    {
        ConversationId = "c-reg",
        Status = ConversationStatus.WaitingForCheckRegistration,
        PendingAction = new PendingAction(
            IntentType.RegisterCheck, "علی کریمی", 5, 50000000,
            MoneyUnit.Toman, new DateTime(2026, 10, 10))
    };

    [Theory]
    [InlineData("بله", BankRegistrationStatus.Registered)]
    [InlineData("خیر", BankRegistrationStatus.NotRegistered)]
    public async Task WaitingForCheckRegistration_WhenAnswered_ShouldRegisterCheck(
        string answer, BankRegistrationStatus expected)
    {
        var (svc, check, store) = Create(WaitingForRegistration());

        var result = await svc.ProcessAsync(new ChatRequestDto("c-reg", answer));

        Assert.Equal(ProcessingStatus.Success, result.Status);
        check.Verify(x => x.RegisterAsync(
            It.Is<RegisterCheckDto>(d => d.Registration == expected && d.CustomerId == 5),
            It.IsAny<CancellationToken>()), Times.Once);
        store.Verify(x => x.RemoveAsync("c-reg", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task WaitingForCheckRegistration_WhenAnswerUnclear_ShouldAskAgain()
    {
        var (svc, check, _) = Create(WaitingForRegistration());

        var result = await svc.ProcessAsync(new ChatRequestDto("c-reg", "شاید"));

        Assert.NotEqual(ProcessingStatus.Success, result.Status);
        check.Verify(x => x.RegisterAsync(
            It.IsAny<RegisterCheckDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
