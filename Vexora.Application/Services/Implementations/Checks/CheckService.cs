using Vexora.Application.Dtos.Checks;
using Vexora.Application.Repositories.Checks;
using Vexora.Application.Services.Interfaces.Checks;
using Vexora.Application.Services.Interfaces.CurrentUser;
using Vexora.Application.UnitOfWorks;
using Vexora.Domain.Entities.Checks;

namespace Vexora.Application.Services.Implementations.Checks;

public sealed class CheckService(
    ICheckRepository checkRepository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork)
    : ICheckService
{
    public async Task RegisterAsync(
        RegisterCheckDto dto,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var check = Check.Create(
            currentUser.UserId,
            dto.CustomerId,
            dto.Amount,
            dto.DueDate,
            dto.CheckNumber);

        await checkRepository.InsertAsync(
            check,
            cancellationToken);

        await unitOfWork.SaveAsync(
            cancellationToken);
    }
}