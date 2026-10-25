using Vexora.Application.Dtos.Customers;
using Vexora.Application.Services.Interfaces.Customers;

namespace Vexora.Application.Test.NLU.ConversationTests.ConversationGuardTestsHelpers;

internal sealed class ThrowingCustomerService : ICustomerService
{
    public Task<CustomerResolutionResultDto> ResolveByNameAsync(string name, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    public Task<CustomerDto> CreateAsync(CreateCustomerDto dto, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();
}
