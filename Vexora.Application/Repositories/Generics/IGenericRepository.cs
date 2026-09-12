namespace Vexora.Application.Repositories.Generics;

public interface IGenericRepository<TEntity> where TEntity : class
{
    //IQueryable<TEntity> GetQueryable(CancellationToken cancellationToken = default);
    Task<TEntity?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task InsertAsync(TEntity entity, CancellationToken cancellationToken = default);
}
