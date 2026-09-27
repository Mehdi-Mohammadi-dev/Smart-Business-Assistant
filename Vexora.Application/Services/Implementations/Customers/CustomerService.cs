using Vexora.Application.Dtos.Customers;
using Vexora.Application.Repositories.Customers;
using Vexora.Application.Services.Interfaces.CurrentUser;
using Vexora.Application.Services.Interfaces.Customers;
using Vexora.Application.UnitOfWorks;
using Vexora.Domain.Entities.Customers;

namespace Vexora.Application.Services.Implementations.Customers;

public sealed class CustomerService(
    ICustomerRepository customerRepository,
    ICurrentUser currentUser,IUnitOfWork unitOfWork)
    : ICustomerService
{
    public async Task<CustomerDto> CreateAsync(
    CreateCustomerDto dto,
    CancellationToken cancellationToken = default)
    {
        var existingCustomer =
            await customerRepository.GetByExactNameAsync(
                currentUser.UserId,
                dto.Name,
                dto.Family,
                cancellationToken);

        if (existingCustomer is not null)
        {
            return new CustomerDto(
                existingCustomer.Id,
                existingCustomer.FirstName,
                existingCustomer.LastName);
        }

        var customer = Customer.Create(
            dto.Name,
            dto.Family,
            currentUser.UserId);

        await customerRepository.InsertAsync(
            customer,
            cancellationToken);

        await unitOfWork.SaveAsync(
            cancellationToken);

        return new CustomerDto(
            customer.Id,
            customer.FirstName,
            customer.LastName);
    }

    public async Task<CustomerResolutionResultDto> ResolveByNameAsync(
    string name,
    CancellationToken cancellationToken = default)
    {
        name = name.Trim();

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

        var exactMatch = candidates.FirstOrDefault(x =>
            string.Equals(
                x.DisplayName,
                name,
                StringComparison.OrdinalIgnoreCase));

        if (exactMatch is not null)
        {
            return new CustomerResolutionResultDto(
                CustomerResolutionStatus.Resolved,
                exactMatch,
                []);
        }

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
