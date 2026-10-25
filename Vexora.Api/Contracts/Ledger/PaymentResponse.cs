using Vexora.Domain.Enums.TransactionDirections;

namespace Vexora.Api.Contracts.Ledger;

public sealed record PaymentResponse(
    long Id,
    long CustomerId,
    string CustomerName,
    decimal Amount,
    DateTimeOffset PaymentDate,
    string PaymentDatePersian,
    TransactionDirection Direction);
