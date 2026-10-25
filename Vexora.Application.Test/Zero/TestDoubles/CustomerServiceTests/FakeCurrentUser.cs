using FluentAssertions;
using Vexora.Application.Dtos.Customers;
using Vexora.Application.Repositories.Customers;
using Vexora.Application.Services.Implementations.Customers;
using Vexora.Application.Services.Interfaces.CurrentUser;
using Vexora.Application.UnitOfWorks;
using Vexora.Domain.Entities.Customers;
using Xunit;

namespace Vexora.Application.Test.Zero.CustomerServiceTestsHelpers;

internal sealed class FakeCurrentUser(long userId) : ICurrentUser
{
    public long UserId { get; } = userId;
}
