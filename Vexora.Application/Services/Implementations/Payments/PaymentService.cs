using Vexora.Application.Dtos.Payments;
using Vexora.Application.Exceptions;
using Vexora.Application.Repositories.Customers;
using Vexora.Application.Repositories.Payments;
using Vexora.Application.Services.Interfaces.CurrentUser;
using Vexora.Application.Services.Interfaces.Payments;
using Vexora.Application.UnitOfWorks;
using Vexora.Domain.Entities.Customers;
using Vexora.Domain.Entities.Payments;

namespace Vexora.Application.Services.Implementations.Payments;

public sealed class PaymentService(ICustomerRepository customerRepository,ICurrentUser currentUser,
    IPaymentRepository paymentRepository,IUnitOfWork unitOfWork) : IPaymentService

{
    public async Task RegisterAsync(RegisterPaymentDto dto, CancellationToken cancellationToken = default)
    {
        var customer = await customerRepository.GetByIdAsync(currentUser.UserId,dto.CustomerId, cancellationToken)??
            throw new EntityNotFoundException(nameof(Customer),dto.CustomerId);

        var payment = Payment.Create(currentUser.UserId, customer.Id, dto.Amount,
            dto.PaymentDate ?? DateTimeOffset.UtcNow);

        await paymentRepository.InsertAsync(payment,cancellationToken);
        await unitOfWork.SaveAsync(cancellationToken);
    }
}
