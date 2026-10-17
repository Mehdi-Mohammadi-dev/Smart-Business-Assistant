using Vexora.Application.Caching.Conversations.Interfaces;
using Vexora.Application.Dtos.Chats;
using Vexora.Application.Dtos.Customers;
using Vexora.Application.NLU.Models.Results;
using Vexora.Application.Services.Interfaces.Chats;

namespace Vexora.Application.Services.Implementations.Chats;

public sealed class ChatService(
    IConversationProcessor conversationProcessor)
    : IChatService
{
    public async Task<ChatResponseDto> ProcessAsync(
        ChatRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result =
            await conversationProcessor.ProcessAsync(
                request,
                cancellationToken);

        var options = ExtractOptions(result.Data);

        return new ChatResponseDto(
            result.Message ?? string.Empty,
            result.Status != ProcessingStatus.Success,
            options);
    }

    private static IReadOnlyCollection<ChatOptionDto> ExtractOptions(
    object? data)
    {
        if (data is not IEnumerable<CustomerCandidateDto> candidates)
            return [];

        var items = candidates.ToArray();

        var options = items
            .Select((customer, index) =>
                new ChatOptionDto(
                    index + 1,
                    customer.DisplayName,
                    ChatOptionType.Customer,
                    customer.CustomerId))
            .ToList();

        options.Add(
            new ChatOptionDto(
                options.Count + 1,
                "ایجاد مشتری جدید",
                ChatOptionType.CreateCustomer,
                null));

        return options;
    }
}
