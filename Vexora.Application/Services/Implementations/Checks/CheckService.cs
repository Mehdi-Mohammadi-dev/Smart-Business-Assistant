using Vexora.Application.Dtos.Checks;
using Vexora.Application.Exceptions;
using Vexora.Application.Repositories.Checks;
using Vexora.Application.Repositories.Customers;
using Vexora.Application.Services.Interfaces.Checks;
using Vexora.Application.Services.Interfaces.CurrentUser;
using Vexora.Application.UnitOfWorks;
using Vexora.Domain.Entities.Checks;
using Vexora.Domain.Entities.Customers;
using Vexora.Domain.Enums.BankRegistrationStatuses;

namespace Vexora.Application.Services.Implementations.Checks;

public sealed class CheckService(
    ICheckRepository checkRepository,
    ICustomerRepository customerRepository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork)
    : ICheckService
{
    public async Task RegisterAsync(
        RegisterCheckDto dto,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var customer = await customerRepository.GetByIdAsync(
            currentUser.UserId,
            dto.CustomerId,
            cancellationToken)
            ?? throw new EntityNotFoundException(
                nameof(Customer),
                dto.CustomerId);

        var check = Check.Create(
            currentUser.UserId,
            dto.CustomerId,
            dto.Amount,
            dto.DueDate,
            dto.CheckNumber,
            bankRegistrationStatus: dto.Registration ??
                                    BankRegistrationStatus.NotRegistered);

        await checkRepository.InsertAsync(
            check,
            cancellationToken);

        await unitOfWork.SaveAsync(
            cancellationToken);
    }
}
