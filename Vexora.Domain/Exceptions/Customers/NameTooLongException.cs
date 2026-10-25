using Vexora.Domain.Constants.Customers;
using Vexora.Domain.sharedKernel.Exceptions;

namespace Vexora.Domain.Exceptions.Customers;

public sealed class NameTooLongException() : AppException(
"CUSTOMER_NAME_TOO_LONG",
$"نام و نام خانوادگی نمی‌تواند بیش از {CustomerConstants.NameLength} نویسه باشد.",
AppErrorType.Validation);
