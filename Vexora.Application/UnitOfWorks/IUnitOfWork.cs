namespace Vexora.Application.UnitOfWorks;

public interface IUnitOfWork
{
    Task<int> SaveAsync(
        CancellationToken cancellationToken = default);


    Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> action,
        CancellationToken cancellationToken = default);
}
