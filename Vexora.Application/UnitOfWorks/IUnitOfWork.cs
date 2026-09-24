using System;
using System.Collections.Generic;
using System.Text;

namespace Vexora.Application.UnitOfWorks;

public interface IUnitOfWork
{
    Task<int> SaveAsync(
        CancellationToken cancellationToken = default);
}
