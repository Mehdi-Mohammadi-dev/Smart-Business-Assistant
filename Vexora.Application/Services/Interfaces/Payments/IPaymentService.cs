using Vexora.Application.Dtos.Payments;

namespace Vexora.Application.Services.Interfaces.Payments;

public interface IPaymentService
{
    Task RegisterAsync(RegisterPaymentDto dto, CancellationToken cancellationToken = default);
}
