using Vexora.Application.Dtos.Customers;
using Vexora.Application.Repositories.Customers;
using Vexora.Application.Services.Interfaces.CurrentUser;
using Vexora.Application.Services.Interfaces.Customers;
using Vexora.Application.UnitOfWorks;
using Vexora.Domain.Entities.Customers;

namespace Vexora.Application.Services.Implementations.Customers;

public sealed class CustomerService(
    ICustomerRepository customerRepository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork)
    : ICustomerService
{
    public async Task<CustomerDto> CreateAsync(
        CreateCustomerDto dto,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = CustomerNameNormalizer.Normalize(dto.Name, dto.Family);

        var existingCustomer =
            await customerRepository.GetByNormalizedNameAsync(
                currentUser.UserId,
                normalizedName,
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
        var normalizedName = CustomerNameNormalizer.Normalize(name);

        if (normalizedName.Length == 0)
        {
            return new CustomerResolutionResultDto(
                CustomerResolutionStatus.NotFound,
                null,
                []);
        }

        var customers = await customerRepository
            .SearchByNameAsync(
                currentUser.UserId,
                normalizedName,
                cancellationToken);

        if (customers.Count == 0)
        {
            return new CustomerResolutionResultDto(
                CustomerResolutionStatus.NotFound,
                null,
                []);
        }

        var exactMatch = customers.FirstOrDefault(x =>
            x.NormalizedName == normalizedName);

        if (exactMatch is not null)
        {
            return new CustomerResolutionResultDto(
                CustomerResolutionStatus.Resolved,
                ToCandidate(exactMatch),
                []);
        }

        var candidates = customers
            .Select(ToCandidate)
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

    private static CustomerCandidateDto ToCandidate(Customer customer)
        => new(customer.Id, customer.FullName);
}
