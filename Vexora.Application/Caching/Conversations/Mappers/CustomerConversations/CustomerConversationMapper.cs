using Vexora.Application.Dtos.Customers;

namespace Vexora.Application.Caching.Conversations.Mappers.CustomerConversations;

public static class CustomerConversationMapper
{
    public static CreateCustomerDto ToCreateCustomerDto(
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
