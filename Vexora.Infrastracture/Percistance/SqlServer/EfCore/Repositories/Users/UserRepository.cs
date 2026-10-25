using Microsoft.EntityFrameworkCore;
using Vexora.Application.Repositories.Users;
using Vexora.Domain.Entities.Users;
using Vexora.Infrastracture.Percistance.SqlServer.EfCore.Context;
using Vexora.Infrastracture.Percistance.SqlServer.EfCore.Repositories.Generics;

namespace Vexora.Infrastracture.Percistance.SqlServer.EfCore.Repositories.Users;

public sealed class UserRepository(ApplicationDbContext context)
    : GenericRepository<User>(context), IUserRepository
{
    public Task<bool> ExistsByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default)
        => context.Users.AnyAsync(x => x.PhoneNumber == phoneNumber, cancellationToken);

    public Task<User?> FindByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default)
        => context.Users.FirstOrDefaultAsync(x => x.PhoneNumber == phoneNumber, cancellationToken);

    public Task<User?> FindByIdAsync(long id, CancellationToken cancellationToken = default)
        => context.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<bool> HasSystemAdminAsync(CancellationToken cancellationToken = default)
        => context.Users.AnyAsync(x => x.Role == Vexora.Domain.Enums.UserRoles.UserRole.SystemAdmin, cancellationToken);

    public async Task<IReadOnlyList<User>> SearchAsync(string? search, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = context.Users.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => (x.FirstName + " " + x.LastName).Contains(search) || x.PhoneNumber.Contains(search));
        return await query.OrderByDescending(x => x.CreateAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
    }
}
