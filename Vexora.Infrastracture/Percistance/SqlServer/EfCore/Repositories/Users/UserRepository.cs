using Microsoft.EntityFrameworkCore;
using Vexora.Application.Repositories.Users;
using Vexora.Domain.Entities.Users;
using Vexora.Infrastracture.Percistance.SqlServer.EfCore.Context;
using Vexora.Infrastracture.Percistance.SqlServer.EfCore.Repositories.Generics;

namespace Vexora.Infrastracture.Percistance.SqlServer.EfCore.Repositories.Users;

public sealed class UserRepository(ApplicationDbContext context) : GenericRepository<User>(context), IUserRepository
{
    public async Task<bool> ExistsByPhneNumberAssync(string phoneNumber, CancellationToken cancellationToken = default)
    {
        return await context.Users.AnyAsync(x => x.PhoneNumber == phoneNumber,cancellationToken);
    }

    public async Task<User> GetByPhoneNumber(string phoneNumber, CancellationToken cancellationToken = default)
    {
        var user= await context.Users.Where(x => x.PhoneNumber == phoneNumber)
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
            throw new Exception("User is NotFound");
        return user;
    }
}
