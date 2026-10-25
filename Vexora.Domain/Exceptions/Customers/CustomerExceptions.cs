using Vexora.Domain.Constants.Customers;
using Vexora.Domain.sharedKernel.Exceptions;

namespace Vexora.Domain.Exceptions.Customers;

public static class CustomerExceptions
{
    public sealed class FirstNameInvalidException() : Exception
        (CustomerConstants.FirstNameInvalid);

    public sealed class LastNameInvalidException() : Exception
    (CustomerConstants.LastNameInvalid);

    public sealed class NameTooLongException() : AppException(
        "CUSTOMER_NAME_TOO_LONG",
        "نام و نام خانوادگی هرکدام نباید بیشتر از ۸۰ کاراکتر باشد.",
        AppErrorType.Validation);

    public sealed class CustomerAlreadyExistsException() : AppException(
        "CUSTOMER_ALREADY_EXISTS",
        "مشتری با این نام قبلاً ثبت شده است. برای تفکیک، یک علامت به نام اضافه کنید (مثلاً «علی رضایی بنا»).",
        AppErrorType.Conflict);
}
