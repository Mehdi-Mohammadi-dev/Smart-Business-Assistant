using Vexora.Application.Dtos.Authentications;

namespace Vexora.Application.Services.Interfaces.Authentications;

public interface IPendingRegistrationStore
{
    Task StoreAsync(
        string token,
        PendingRegistration registration,
        TimeSpan lifetime,
        CancellationToken cancellationToken = default);

    Task<PendingRegistration?> GetAsync(
        string token,
        CancellationToken cancellationToken = default);

    Task RemoveAsync(
        string token,
        CancellationToken cancellationToken = default);
}
