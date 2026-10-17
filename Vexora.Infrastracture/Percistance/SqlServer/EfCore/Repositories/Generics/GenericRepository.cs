using Microsoft.EntityFrameworkCore;
using Vexora.Application.Repositories.Generics;
using Vexora.Infrastracture.Percistance.SqlServer.EfCore.Context;

namespace Vexora.Infrastracture.Percistance.SqlServer.EfCore.Repositories.Generics;

public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
{
    protected readonly ApplicationDbContext _db;
    protected readonly DbSet<TEntity> _dbset;

    public GenericRepository(ApplicationDbContext context)
    {
        _db = context;
        _dbset = _db.Set<TEntity>();
    }

    public async Task<TEntity?> GetByIdAsync(long userId,long id, CancellationToken cancellationToken = default)
    {
        return await _dbset
       .Where(x =>
           EF.Property<long>(x, "UserId") == userId &&
           EF.Property<long>(x, "Id") == id)
       .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
            await _dbset.AddAsync(entity, cancellationToken);
    }
}
