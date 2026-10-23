using FluentAssertions;
using Vexora.Application.Caching.Conversations;
using Vexora.Application.Caching.Conversations.Implementations;
using Vexora.Application.Caching.Conversations.Interfaces;
using Vexora.Application.Constants.Messages.Conversations;
using Vexora.Application.Dtos.Checks;
using Vexora.Application.Dtos.Customers;
using Vexora.Application.Dtos.Payments;
using Vexora.Application.NLU.Abstractions.Dates;
using Vexora.Application.NLU.Abstractions.Pipeline;
using Vexora.Application.NLU.Models.Intents;
using Vexora.Application.NLU.Models.Results;
using Vexora.Application.Services.Interfaces.Checks;
using Vexora.Application.Services.Interfaces.Customers;
using Vexora.Application.Services.Interfaces.Payments;
using Xunit;

namespace Vexora.Application.Test.NLU.ConversationTests;

public sealed class ConversationPendingTests
{
    private readonly InMemoryStateStore _store = new();
    private readonly ConversationProcessor _processor;

    public ConversationPendingTests()
    {
        _processor = new ConversationProcessor(
            new ThrowingNluPipeline(),
            _store,
            new ThrowingCustomerService(),
            new ThrowingPaymentService(),
            new ThrowingCheckService(),
            new ThrowingDateExtractor());
    }

    [Fact]
    public void Conversation_Id_Should_Be_Built_From_User_Id()
    {
        ConversationIds.ForUser(7).Should().Be(ConversationIds.ForUser(7));
        ConversationIds.ForUser(7).Should().NotBe(ConversationIds.ForUser(8));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Conversation_Id_Should_Reject_Invalid_User_Id(long userId)
    {
        var action = () => ConversationIds.ForUser(userId);

        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task Pending_Should_Be_Null_When_Nothing_Is_Waiting()
    {
        var result = await _processor.GetPendingAsync(ConversationIds.ForUser(1));

        result.Should().BeNull();
    }

    [Fact]
    public async Task Pending_Should_Ask_For_Check_Due_Date()
    {
        await SaveAsync(1, ConversationStatus.WaitingForCheckDate);

        var result = await _processor.GetPendingAsync(ConversationIds.ForUser(1));

        result!.Message.Should().Be(ConversationMessages.CheckDueDateRequired);
        result.Status.Should().Be(ProcessingStatus.Failed);
    }

    [Fact]
    public async Task Pending_Should_Return_Customer_Candidates()
    {
        await SaveAsync(
            1,
            ConversationStatus.WaitingForCustomerSelection,
            candidates: [new CustomerCandidateDto(10, "علی رضایی"), new CustomerCandidateDto(11, "علی احمدی")]);

        var result = await _processor.GetPendingAsync(ConversationIds.ForUser(1));

        result!.Message.Should().Be(ConversationMessages.CustomerSelectionRequired);
        ((IReadOnlyCollection<CustomerCandidateDto>)result.Data!).Should().HaveCount(2);
    }

    [Fact]
    public async Task Pending_Should_Ask_To_Confirm_Customer_Creation()
    {
        await SaveAsync(
            1,
            ConversationStatus.WaitingForCustomerCreationConfirmation,
            new PendingAction(IntentType.RegisterPayment, "حسین", null, 1000, null, null));

        var result = await _processor.GetPendingAsync(ConversationIds.ForUser(1));

        result!.Message.Should().Be(ConversationMessages.CustomerNotFound("حسین"));
    }

    [Fact]
    public async Task Pending_Should_Ask_For_New_Customer_Name()
    {
        await SaveAsync(1, ConversationStatus.WaitingForCustomerName);

        var result = await _processor.GetPendingAsync(ConversationIds.ForUser(1));

        result!.Message.Should().Be(ConversationMessages.CustomerNameEnter);
    }

    [Fact]
    public async Task Pending_Should_Not_Be_Shared_Between_Users()
    {
        await SaveAsync(1, ConversationStatus.WaitingForCheckDate);

        var otherUser = await _processor.GetPendingAsync(ConversationIds.ForUser(2));

        otherUser.Should().BeNull();
    }

    [Fact]
    public async Task Reset_Should_Remove_Pending_State()
    {
        await SaveAsync(1, ConversationStatus.WaitingForCheckDate);

        await _processor.ResetAsync(ConversationIds.ForUser(1));

        var result = await _processor.GetPendingAsync(ConversationIds.ForUser(1));

        result.Should().BeNull();
    }

    private Task SaveAsync(
        long userId,
        ConversationStatus status,
        PendingAction? pending = null,
        IReadOnlyCollection<CustomerCandidateDto>? candidates = null)
    {
        return _store.SaveAsync(new ConversationState
        {
            ConversationId = ConversationIds.ForUser(userId),
            Status = status,
            PendingAction = pending,
            CustomerCandidates = candidates ?? []
        });
    }

    private sealed class InMemoryStateStore : IConversationStateStore
    {
        private readonly Dictionary<string, ConversationState> _states = new();

        public Task<ConversationState?> GetAsync(string conversationId, CancellationToken cancellationToken = default)
            => Task.FromResult(_states.GetValueOrDefault(conversationId));

        public Task SaveAsync(ConversationState state, CancellationToken cancellationToken = default)
        {
            _states[state.ConversationId] = state;
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string conversationId, CancellationToken cancellationToken = default)
        {
            _states.Remove(conversationId);
            return Task.CompletedTask;
        }
    }

    private sealed class ThrowingNluPipeline : INluPipeline
    {
        public NluResult Analyze(string text) => throw new NotSupportedException();
    }

    private sealed class ThrowingCustomerService : ICustomerService
    {
        public Task<CustomerResolutionResultDto> ResolveByNameAsync(string name, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<CustomerDto> CreateAsync(CreateCustomerDto dto, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private sealed class ThrowingPaymentService : IPaymentService
    {
        public Task RegisterAsync(RegisterPaymentDto dto, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private sealed class ThrowingCheckService : ICheckService
    {
        public Task RegisterAsync(RegisterCheckDto dto, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private sealed class ThrowingDateExtractor : IDateExtractor
    {
        public DateTime? Extract(string text) => throw new NotSupportedException();
    }
}
