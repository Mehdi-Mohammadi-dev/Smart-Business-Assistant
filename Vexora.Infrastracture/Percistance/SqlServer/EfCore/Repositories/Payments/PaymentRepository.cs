using Vexora.Application.Repositories.Payments;
using Vexora.Domain.Entities.Payments;
using Vexora.Infrastracture.Percistance.SqlServer.EfCore.Context;
using Vexora.Infrastracture.Percistance.SqlServer.EfCore.Repositories.Generics;

namespace Vexora.Infrastracture.Percistance.SqlServer.EfCore.Repositories.Payments;

public sealed class PaymentRepository(ApplicationDbContext context):GenericRepository<Payment>(context),IPaymentRepository
{
}
