using Vexora.Domain.Constants.Customers;
using Vexora.Domain.sharedKernel.Exceptions;

namespace Vexora.Domain.Exceptions.Customers;

public static class CustomerExceptions
{
    public sealed class FirstNameInvalidException() : Exception(CustomerConstants.FirstNameInvalid);

    public sealed class LastNameInvalidException() : Exception(CustomerConstants.LastNameInvalid);

    public sealed class NameTooLongException() : AppException(
        "CUSTOMER_NAME_TOO_LONG",
        $"نام و نام خانوادگی نمی‌تواند بیش از {CustomerConstants.NameLength} نویسه باشد.",
        AppErrorType.Validation);

    public sealed class CustomerAlreadyExistsException() : AppException(
        "CUSTOMER_ALREADY_EXISTS",
        "مشتری با همین نام قبلاً ثبت شده است.",
        AppErrorType.Conflict);
}
