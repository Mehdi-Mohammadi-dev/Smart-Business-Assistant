using Vexora.Application.NLU.Models.Intents;
using Vexora.Application.NLU.Models.Money;
using Vexora.Domain.Enums.BankRegistrationStatuses;

namespace Vexora.Application.Caching.Conversations;

public sealed record PendingAction
(
    IntentType Intent,

    string? CustomerName,

    long? CustomerId,

    decimal? Amount,

    MoneyUnit? MoneyUnit,

    DateTime? Date,

    string? CheckNumber = null,

    BankRegistrationStatus? Registration = null
);
