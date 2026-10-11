using Vexora.Application.Repositories.Generics;
using Vexora.Domain.Entities.Users;

namespace Vexora.Application.Repositories.Users;

public interface IUserRepository:IGenericRepository<User>
{
    public Task<User> GetByPhoneNumber(string phoneNumber,CancellationToken cancellationToken=default);

    public Task<bool> ExistsByPhneNumberAssync(string phoneNumber,CancellationToken cancellationToken=default);
}
