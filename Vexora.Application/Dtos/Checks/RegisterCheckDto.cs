using Vexora.Domain.Enums.BankRegistrationStatuses;

namespace Vexora.Application.Dtos.Checks;

public sealed record RegisterCheckDto
(
    long CustomerId,
    decimal Amount,
    DateTime DueDate,
    string? CheckNumber,
    BankRegistrationStatus? Registration = null
);
