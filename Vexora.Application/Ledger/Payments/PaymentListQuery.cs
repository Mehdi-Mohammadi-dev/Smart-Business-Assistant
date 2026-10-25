namespace Vexora.Application.Ledger.Payments;

public sealed record PaymentListQuery(
    long? CustomerId,
    DateTime? From,
    DateTime? To,
    int Page,
    int PageSize);
