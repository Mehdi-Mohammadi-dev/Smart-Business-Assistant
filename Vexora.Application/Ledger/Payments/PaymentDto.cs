using Vexora.Domain.Enums.TransactionDirections;

namespace Vexora.Application.Ledger.Payments;

public sealed record PaymentDto(
    long Id,
    long CustomerId,
    string CustomerName,
    decimal Amount,
    DateTimeOffset PaymentDate,
    TransactionDirection Direction);
