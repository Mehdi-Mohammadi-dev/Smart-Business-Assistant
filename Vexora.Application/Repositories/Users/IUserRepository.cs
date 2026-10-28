using Vexora.Application.Repositories.Generics;
using Vexora.Domain.Entities.Users;

namespace Vexora.Application.Repositories.Users;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> FindByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default);

    Task<User?> FindByIdAsync(long id, CancellationToken cancellationToken = default);

    Task<bool> ExistsByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default);
    Task<User?> FindByInviteCodeAsync(string inviteCode, CancellationToken cancellationToken = default);
    Task<bool> ExistsByInviteCodeAsync(string inviteCode, CancellationToken cancellationToken = default);
    Task<bool> HasSystemAdminAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> SearchAsync(string? search, int page, int pageSize, CancellationToken cancellationToken = default);
}
