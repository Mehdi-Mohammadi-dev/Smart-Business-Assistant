using FluentAssertions;
using Vexora.Application.Caching.Conversations;
using Vexora.Application.Caching.Conversations.Implementations;
using Vexora.Application.Caching.Conversations.Interfaces;
using Vexora.Application.Constants.Messages.Conversations;
using Vexora.Application.Dtos.Chats;
using Vexora.Application.Dtos.Checks;
using Vexora.Application.Dtos.Customers;
using Vexora.Application.Dtos.Payments;
using Vexora.Application.NLU.Extractions;
using Vexora.Application.NLU.Extractions.Customers;
using Vexora.Application.NLU.Extractions.Dates;
using Vexora.Application.NLU.Extractions.Pipelines;
using Vexora.Application.NLU.Intent;
using Vexora.Application.NLU.Models.Intents;
using Vexora.Application.NLU.Models.Results;
using Vexora.Application.NLU.Normalizations;
using Vexora.Application.NLU.Normalizations.Parsing.Money;
using Vexora.Application.NLU.Parsing.Number;
using Vexora.Application.NLU.Pipeline;
using Vexora.Application.Services.Interfaces.Checks;
using Vexora.Application.Services.Interfaces.Customers;
using Vexora.Application.Services.Interfaces.Payments;
using Xunit;

using Vexora.Application.Test.NLU.ConversationTests.ConversationOverrideTestsHelpers;

namespace Vexora.Application.Test.NLU.ConversationTests;

public sealed class ConversationOverrideTests
{
    private const string ConversationId = "user-1";

    private readonly InMemoryStateStore _store = new();
    private readonly ConversationProcessor _processor;

    public ConversationOverrideTests()
    {
        var pipeline = new NluPipeline(
            new PersianTextNormalizer(),
            new IntentDetector(),
            new EntityExtractionPipeline(
                new MoneyExtractor(new MoneyParser(new PersianNumberParser())),
                new CustomerExtractor(),
                new PersianDateExtractor()));

        _processor = new ConversationProcessor(
            pipeline,
            _store,
            new NotFoundCustomerService(),
            new ThrowingPaymentService(),
            new ThrowingCheckService(),
            new PersianDateExtractor());
    }

    [Theory]
    [InlineData("لغو")]
    [InlineData("انصراف")]
    [InlineData("بیخیال")]
    [InlineData("بی‌خیال شو")]
    [InlineData("کنسل")]
    public async Task Cancel_Word_Should_Clear_Any_Waiting_State(
        string message)
    {
        await StartWaitingForCustomerCreationAsync();

        var result = await _processor.ProcessAsync(new ChatRequestDto(ConversationId, message));

        result.Status.Should().Be(ProcessingStatus.Success);
        result.Message.Should().Be(ConversationMessages.ConversationCancelled);
        (await _store.GetAsync(ConversationId)).Should().BeNull();
    }

    [Fact]
    public async Task A_Refused_New_Command_Should_Not_Be_Swallowed_By_The_Old_Question()
    {
        await StartWaitingForCustomerCreationAsync();

        var result = await _processor.ProcessAsync(
            new ChatRequestDto(ConversationId, "به علی رضایی 500 هزار تومان دادم"));

        result.Message.Should().Be(ConversationMessages.OnlyReceivedSupported);
        (await _store.GetAsync(ConversationId)).Should().BeNull();
    }

    [Fact]
    public async Task A_New_Command_Should_Replace_The_Waiting_State()
    {
        await _store.SaveAsync(new ConversationState
        {
            ConversationId = ConversationId,
            Status = ConversationStatus.WaitingForCheckDate,
            PendingAction = new PendingAction(IntentType.RegisterCheck, "رضا", null, 5_000_000, null, null)
        });

        var result = await _processor.ProcessAsync(
            new ChatRequestDto(ConversationId, "حسین 200 هزار تومان داد"));

        result.Message.Should().Be(ConversationMessages.CustomerNotFound("حسین"));

        var state = await _store.GetAsync(ConversationId);

        state!.Status.Should().Be(ConversationStatus.WaitingForCustomerCreationConfirmation);
        state.PendingAction!.CustomerName.Should().Be("حسین");
        state.PendingAction.Amount.Should().Be(200_000);
    }

    [Fact]
    public async Task A_Normal_Answer_Should_Still_Continue_The_Old_Question()
    {
        await StartWaitingForCustomerCreationAsync();

        var result = await _processor.ProcessAsync(new ChatRequestDto(ConversationId, "خیر"));

        result.Message.Should().Be(ConversationMessages.CustomerCreationCancelled);
    }

    private async Task StartWaitingForCustomerCreationAsync()
    {
        await _store.SaveAsync(new ConversationState
        {
            ConversationId = ConversationId,
            Status = ConversationStatus.WaitingForCustomerCreationConfirmation,
            PendingAction = new PendingAction(IntentType.RegisterPayment, "کارت", null, 100_000, null, null)
        });
    }




}
