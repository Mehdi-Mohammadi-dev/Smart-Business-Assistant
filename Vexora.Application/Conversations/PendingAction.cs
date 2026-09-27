using Vexora.Application.NLU.Models.Intents;
using Vexora.Application.NLU.Models.Money;

namespace Vexora.Application.Conversations;

public sealed record PendingAction
(
    IntentType Intent,

    string? CustomerName,

    long? CustomerId,

    decimal? Amount,

    MoneyUnit? MoneyUnit,

   DateTime? Date
);
