using Vexora.Domain.Enums.BankRegistrationStatuses;

namespace Vexora.Api.Contracts.Ledger;

public sealed class CheckRegistrationRequest
{
    public BankRegistrationStatus BankRegistration { get; set; }
}
