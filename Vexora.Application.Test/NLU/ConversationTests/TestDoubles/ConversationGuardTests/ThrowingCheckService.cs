using Vexora.Application.Dtos.Checks;
using Vexora.Application.Services.Interfaces.Checks;

namespace Vexora.Application.Test.NLU.ConversationTests.ConversationGuardTestsHelpers;

internal sealed class ThrowingCheckService : ICheckService
{
    public Task RegisterAsync(RegisterCheckDto dto, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();
}
