using Vexora.Application.Dtos.Customers;
using Vexora.Application.Repositories.Customers;
using Vexora.Application.Services.Interfaces.CurrentUser;
using Vexora.Application.Services.Interfaces.Customers;
using Vexora.Application.UnitOfWorks;
using Vexora.Domain.Entities.Customers;
using Vexora.Domain.Validators.Customers;
using CustomerNameNormalizer = Vexora.Domain.Entities.Customers.CustomerNameNormalizer;

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
        var existingCustomer = await customerRepository.GetByExactNameAsync(
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

        await customerRepository.InsertAsync(customer, cancellationToken);
        await unitOfWork.SaveAsync(cancellationToken);

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

        // Do not search for an empty string: every string contains "".
        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            return new CustomerResolutionResultDto(
                CustomerResolutionStatus.NotFound,
                null,
                []);
        }

        var customers = await customerRepository.SearchByNameAsync(
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

        var exactCustomer = customers.FirstOrDefault(customer =>
            CustomerNameNormalizer.Normalize(
                customer.FirstName,
                customer.LastName) == normalizedName);

        if (exactCustomer is not null)
        {
            var exactCandidate = new CustomerCandidateDto(
                exactCustomer.Id,
                $"{exactCustomer.FirstName} {exactCustomer.LastName}".Trim());

            return new CustomerResolutionResultDto(
                CustomerResolutionStatus.Resolved,
                exactCandidate,
                []);
        }

        var candidates = customers
            .Select(customer => new CustomerCandidateDto(
                customer.Id,
                $"{customer.FirstName} {customer.LastName}".Trim()))
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