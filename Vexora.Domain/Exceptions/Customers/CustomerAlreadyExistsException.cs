using Vexora.Domain.Constants.Customers;
using Vexora.Domain.sharedKernel.Exceptions;

namespace Vexora.Domain.Exceptions.Customers;

public sealed class CustomerAlreadyExistsException() : AppException(
"CUSTOMER_ALREADY_EXISTS",
"مشتری با همین نام قبلاً ثبت شده است.",
AppErrorType.Conflict);
