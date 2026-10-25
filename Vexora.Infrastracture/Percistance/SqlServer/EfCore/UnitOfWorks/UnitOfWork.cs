using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Vexora.Application.UnitOfWorks;
using Vexora.Domain.Exceptions.Customers;
using Vexora.Infrastracture.Percistance.SqlServer.EfCore.Configurations.Customers;
using Vexora.Infrastracture.Percistance.SqlServer.EfCore.Context;

namespace Vexora.Infrastracture.Percistance.SqlServer.EfCore.UnitOfWorks;

public sealed class UnitOfWork(ApplicationDbContext context) : IUnitOfWork
{
    public async Task<int> SaveAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsDuplicateCustomerName(exception))
        {
            throw new CustomerExceptions.CustomerAlreadyExistsException();
        }
    }

    private static bool IsDuplicateCustomerName(DbUpdateException exception)
    {
        return exception.InnerException is SqlException { Number: 2601 or 2627 } sqlException &&
               sqlException.Message.Contains(
                   CustomerConfiguration.NormalizedNameIndexName,
                   StringComparison.Ordinal);
    }

    public async Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> action,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        await action(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }
}
