using Vexora.Application.Dtos.Customers;

namespace Vexora.Application.Services.Interfaces.Customers;

public interface ICustomerService
{
    Task<CustomerResolutionResultDto> ResolveByNameAsync(string name, CancellationToken cancellationToken = default);

    Task<CustomerDto> CreateAsync(
    CreateCustomerDto dto,
    CancellationToken cancellationToken = default);
}
