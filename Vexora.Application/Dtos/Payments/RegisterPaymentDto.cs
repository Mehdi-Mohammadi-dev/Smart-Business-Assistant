namespace Vexora.Application.Dtos.Payments;

public sealed record RegisterPaymentDto
(
    long CustomerId ,

    decimal Amount ,

    DateTime? PaymentDate
);
