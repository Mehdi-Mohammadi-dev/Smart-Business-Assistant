using Vexora.Application.Dtos.Checks;

namespace Vexora.Application.Services.Interfaces.Checks;

public interface ICheckService
{
    Task RegisterAsync(
        RegisterCheckDto dto,
        CancellationToken cancellationToken = default);
}
