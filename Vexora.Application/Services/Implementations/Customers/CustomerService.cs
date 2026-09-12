using Vexora.Application.Dtos.Customers;
using Vexora.Application.Repositories.Customers;
using Vexora.Application.Services.Interfaces.CurrentUser;
using Vexora.Application.Services.Interfaces.Customers;

namespace Vexora.Application.Services.Implementations.Customers;

public sealed class CustomerService(ICustomerRepository customerRepository,ICurrentUser currentUser):ICustomerService
{
    public async Task<CustomerResolutionResultDto> SearchByName(string name,CancellationToken cancellationToken=default)
    {

        var customers = await customerRepository.SearchByNameAsync(currentUser.UserId,name,cancellationToken);

        if (customers.Count == 0)
        {
            return new CustomerResolutionResultDto
            (
                CustomerResolutionStatus.NotFound,null,[]
            );
        }

        if (customers.Count == 1)
        {
            var customer = customers.First();

            return new CustomerResolutionResultDto
            (
                CustomerResolutionStatus.Resolved,
                new CustomerCandidateDto
                (
                    customer.Id,
                    customer.FirstName+" "+customer.LastName
                ), []
            );
        }
        var condidate = customers.Select(x => new CustomerCandidateDto
                (
                    x.Id, x.FirstName + " " + x.LastName
                ));
        return new CustomerResolutionResultDto
        (
            CustomerResolutionStatus.Ambiguous, null,
            customers.Select(x => new CustomerCandidateDto
                (
                    x.Id, x.FirstName+" "+ x.LastName
                )).ToArray()
        );
    }
}
