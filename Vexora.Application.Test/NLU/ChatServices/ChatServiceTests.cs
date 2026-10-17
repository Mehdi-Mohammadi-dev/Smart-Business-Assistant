using Moq;
using Vexora.Application.Caching.Conversations.Interfaces;
using Vexora.Application.Dtos.Chats;
using Vexora.Application.Dtos.Customers;
using Vexora.Application.NLU.Models.Results;
using Vexora.Application.Services.Implementations.Chats;
using Xunit;

namespace Vexora.Application.Test.NLU.ChatServices;

public sealed class ChatServiceTests
{
    [Fact]
    public async Task ProcessAsync_WhenProcessorSucceeds_ShouldReturnSuccessfulResponse()
    {
        var conversationProcessor =
            new Mock<IConversationProcessor>();

        conversationProcessor
            .Setup(x => x.ProcessAsync(
                It.IsAny<ChatRequestDto>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new ConversationProcessingResult(
                    ProcessingStatus.Success,
                    "درخواست شناسایی شد.",
                    null));

        var service = new ChatService(
            conversationProcessor.Object);

        var request = new ChatRequestDto(
            "conversation-1",
            "از مهدی 150 تومن گرفتم");

        var result = await service.ProcessAsync(request);

        Assert.NotNull(result);
        Assert.Equal(
            "درخواست شناسایی شد.",
            result.Message);

        Assert.False(result.RequiresUserInput);

        conversationProcessor.Verify(
            x => x.ProcessAsync(
                request,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessAsync_WhenProcessorFails_ShouldRequireUserInput()
    {
        var conversationProcessor =
            new Mock<IConversationProcessor>();

        conversationProcessor
            .Setup(x => x.ProcessAsync(
                It.IsAny<ChatRequestDto>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new ConversationProcessingResult(
                    ProcessingStatus.Failed,
                    "متوجه درخواست شما نشدم.",
                    null));

        var service = new ChatService(
            conversationProcessor.Object);

        var request = new ChatRequestDto(
            "conversation-1",
            "asdfgh");

        var result = await service.ProcessAsync(request);

        Assert.NotNull(result);
        Assert.Equal(
            "متوجه درخواست شما نشدم.",
            result.Message);

        Assert.True(result.RequiresUserInput);

        conversationProcessor.Verify(
            x => x.ProcessAsync(
                request,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessAsync_ShouldPassCancellationTokenToProcessor()
    {
        var conversationProcessor =
            new Mock<IConversationProcessor>();

        conversationProcessor
            .Setup(x => x.ProcessAsync(
                It.IsAny<ChatRequestDto>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new ConversationProcessingResult(
                    ProcessingStatus.Success,
                    "درخواست شناسایی شد.",
                    null));

        var service = new ChatService(
            conversationProcessor.Object);

        var request = new ChatRequestDto(
            "conversation-1",
            "سلام");

        using var cts =
            new CancellationTokenSource();

        await service.ProcessAsync(
            request,
            cts.Token);

        conversationProcessor.Verify(
            x => x.ProcessAsync(
                request,
                cts.Token),
            Times.Once);
    }

    [Fact]
    public async Task ProcessAsync_WhenProcessorReturnsUnknownIntentFailure_ShouldReturnRequiresUserInput()
    {
        var conversationProcessor =
            new Mock<IConversationProcessor>();

        conversationProcessor
            .Setup(x => x.ProcessAsync(
                It.IsAny<ChatRequestDto>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new ConversationProcessingResult(
                    ProcessingStatus.Failed,
                    "متوجه درخواست شما نشدم.",
                    null));

        var service = new ChatService(
            conversationProcessor.Object);

        var request = new ChatRequestDto(
            "conversation-1",
            "یک متن نامفهوم");

        var result = await service.ProcessAsync(request);

        Assert.Equal(
            "متوجه درخواست شما نشدم.",
            result.Message);

        Assert.True(result.RequiresUserInput);
    }

    [Fact]
    public async Task ProcessAsync_WhenCandidatesExist_ShouldMapCandidateNamesToOptions()
    {
        var candidates = new[]
        {
        new CustomerCandidateDto(1, "مهدی محمدی"),
        new CustomerCandidateDto(2, "مهدی احمدی")
    };

        var processor = new Mock<IConversationProcessor>();

        processor
            .Setup(x => x.ProcessAsync(
                It.IsAny<ChatRequestDto>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new ConversationProcessingResult(
                    ProcessingStatus.Failed,
                    "لطفاً مشتری موردنظر را انتخاب کنید.",
                    candidates));

        var service = new ChatService(processor.Object);

        var request = new ChatRequestDto(
            "conversation-1",
            "از مهدی 150 تومن گرفتم");

        var result = await service.ProcessAsync(request);

        Assert.Equal(
            "لطفاً مشتری موردنظر را انتخاب کنید.",
            result.Message);

        Assert.True(result.RequiresUserInput);

        Assert.Equal(3, result.Options.Count);

        Assert.Equal(1, result.Options.ElementAt(0).Number);
        Assert.Equal("مهدی محمدی", result.Options.ElementAt(0).Label);
        Assert.Equal(ChatOptionType.Customer, result.Options.ElementAt(0).Type);
        Assert.Equal(1, result.Options.ElementAt(0).CustomerId);

        Assert.Equal(2, result.Options.ElementAt(1).Number);
        Assert.Equal("مهدی احمدی", result.Options.ElementAt(1).Label);
        Assert.Equal(ChatOptionType.Customer, result.Options.ElementAt(1).Type);
        Assert.Equal(2, result.Options.ElementAt(1).CustomerId);

        Assert.Equal(3, result.Options.ElementAt(2).Number);
        Assert.Equal("ایجاد مشتری جدید", result.Options.ElementAt(2).Label);
        Assert.Equal(ChatOptionType.CreateCustomer, result.Options.ElementAt(2).Type);
        Assert.Null(result.Options.ElementAt(2).CustomerId);
    }

    [Fact]
    public async Task ProcessAsync_WhenNoOptionsExist_ShouldReturnEmptyOptions()
    {
        var processor = new Mock<IConversationProcessor>();

        processor
            .Setup(x => x.ProcessAsync(
                It.IsAny<ChatRequestDto>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new ConversationProcessingResult(
                    ProcessingStatus.Success,
                    "پرداخت با موفقیت ثبت شد.",
                    null));

        var service = new ChatService(processor.Object);

        var request = new ChatRequestDto(
            "conversation-1",
            "از مهدی 150 تومن گرفتم");

        var result = await service.ProcessAsync(request);

        Assert.Equal(
            "پرداخت با موفقیت ثبت شد.",
            result.Message);

        Assert.False(result.RequiresUserInput);

        Assert.Empty(result.Options);
    }
}
