using Vexora.Application.Dtos.Customers;
using Vexora.Application.Repositories.Customers;
using Vexora.Application.Services.Interfaces.CurrentUser;
using Vexora.Application.Services.Interfaces.Customers;

namespace Vexora.Application.Services.Implementations.Customers;

public sealed class CustomerService(
    ICustomerRepository customerRepository,
    ICurrentUser currentUser)
    : ICustomerService
{
    public async Task<CustomerResolutionResultDto> ResolveByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        var customers = await customerRepository
            .SearchByNameAsync(
                currentUser.UserId,
                name,
                cancellationToken);

        if (customers.Count == 0)
        {
            return new CustomerResolutionResultDto(
                CustomerResolutionStatus.NotFound,
                null,
                []);
        }


        var candidates = customers
            .Select(x => new CustomerCandidateDto(
                x.Id,
                $"{x.FirstName} {x.LastName}".Trim()))
            .ToArray();


        if (candidates.Length == 1)
        {
            return new CustomerResolutionResultDto(
                CustomerResolutionStatus.Resolved,
                candidates[0],
                []);
        }


        return new CustomerResolutionResultDto(
            CustomerResolutionStatus.Ambiguous,
            null,
            candidates);
    }
}
