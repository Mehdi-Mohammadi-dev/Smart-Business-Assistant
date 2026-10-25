namespace Vexora.Application.Caching.Authentications;

public interface ILoginAttemptStore
{
    Task<bool> IsLockedAsync(string phoneNumber, CancellationToken cancellationToken = default);
    Task RegisterFailureAsync(string phoneNumber, CancellationToken cancellationToken = default);
    Task ClearAsync(string phoneNumber, CancellationToken cancellationToken = default);
}
