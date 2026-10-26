using FluentAssertions;
using Vexora.Application.Caching.Conversations.Implementations;
using Vexora.Application.Constants.Messages.Conversations;
using Vexora.Application.Dtos.Chats;
using Vexora.Application.NLU.Extractions;
using Vexora.Application.NLU.Extractions.Customers;
using Vexora.Application.NLU.Extractions.Dates;
using Vexora.Application.NLU.Extractions.Pipelines;
using Vexora.Application.NLU.Intent;
using Vexora.Application.NLU.Models.Results;
using Vexora.Application.NLU.Normalizations;
using Vexora.Application.NLU.Normalizations.Parsing.Money;
using Vexora.Application.NLU.Parsing.Number;
using Vexora.Application.NLU.Pipeline;
using Vexora.Application.Test.NLU.ConversationTests.ConversationGuardTestsHelpers;
using Xunit;

namespace Vexora.Application.Test.NLU.ConversationTests;

public sealed class ConversationGuardTests
{
    private readonly ConversationProcessor _processor;

    public ConversationGuardTests()
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
            new EmptyStateStore(),
            new ThrowingCustomerService(),
            new ThrowingPaymentService(),
            new ThrowingCheckService(),
            new PersianDateExtractor(),
            new global::Moq.Mock<global::Vexora.Application.Ledger.Customers.ILedgerCustomerService>().Object);
    }

    [Theory]
    [InlineData("به علی رضایی 500 هزار تومان دادم")]
    [InlineData("علی رضایی 500 هزار تومان گرفت")]
    [InlineData("به رضا یک چک 5 میلیونی دادم")]
    public async Task Should_Refuse_When_The_User_Paid_Instead_Of_Received(
        string message)
    {
        var result = await ProcessAsync(message);

        result.Status.Should().Be(ProcessingStatus.Failed);
        result.Message.Should().Be(ConversationMessages.OnlyReceivedSupported);
    }

    [Theory]
    [InlineData("چک 5 میلیون تومانی و 2 میلیون تومان نقد از علی گرفتم")]
    [InlineData("علی 500 هزار تومان داد و 200 هزار تومان")]
    public async Task Should_Ask_To_Split_When_Message_Has_More_Than_One_Amount(
        string message)
    {
        var result = await ProcessAsync(message);

        result.Status.Should().Be(ProcessingStatus.Failed);
        result.Message.Should().Be(ConversationMessages.MultipleAmounts);
    }

    [Theory]
    [InlineData("به علی 10 میلیون جنس دادم")]
    [InlineData("علی چقدر بدهکاره")]
    public async Task Should_Say_Clearly_That_Unsupported_Requests_Are_Not_Done(
        string message)
    {
        var result = await ProcessAsync(message);

        result.Status.Should().Be(ProcessingStatus.Failed);
        result.Message.Should().Be(ConversationMessages.UnsupportedRequest);
    }

    private Task<ConversationProcessingResult> ProcessAsync(string message)
        => _processor.ProcessAsync(new ChatRequestDto("user-1", message));




}
