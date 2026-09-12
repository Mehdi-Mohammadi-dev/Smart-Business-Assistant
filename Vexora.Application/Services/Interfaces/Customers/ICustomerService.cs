using Vexora.Application.Dtos.Customers;

namespace Vexora.Application.Services.Interfaces.Customers;

public interface ICustomerService
{
    Task<CustomerResolutionResultDto> SearchByName(string name, CancellationToken cancellationToken = default);
}
