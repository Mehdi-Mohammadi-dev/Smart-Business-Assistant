namespace Vexora.Application.Repositories.Generics;

public interface IGenericRepository<TEntity> where TEntity : class
{
    Task<TEntity?> GetByIdAsync(long userId,long id, CancellationToken cancellationToken = default);
    Task InsertAsync(TEntity entity, CancellationToken cancellationToken = default);
}
