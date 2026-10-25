using FluentAssertions;
using Vexora.Application.Dtos.Customers;
using Vexora.Application.Repositories.Customers;
using Vexora.Application.Services.Implementations.Customers;
using Vexora.Application.Services.Interfaces.CurrentUser;
using Vexora.Application.UnitOfWorks;
using Vexora.Domain.Entities.Customers;
using Xunit;

namespace Vexora.Application.Test.Zero.CustomerServiceTestsHelpers;

internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveCount { get; private set; }

    public Task<int> SaveAsync(CancellationToken cancellationToken = default)
    {
        SaveCount++;
        return Task.FromResult(1);
    }

    public Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> action,
        CancellationToken cancellationToken = default)
        => action(cancellationToken);
}
