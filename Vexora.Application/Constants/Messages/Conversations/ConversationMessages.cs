namespace Vexora.Application.Constants.Messages.Conversations;

public static class ConversationMessages
{
    public const string UnknownRequest =
        "متوجه درخواست شما نشدم.";

    public const string ConversationCancelled =
        "ثبت قبلی لغو شد. هر وقت خواستید دوباره تایپ کنید.";

    public const string OnlyReceivedSupported =
        "در این نسخه فقط «دریافتی» ثبت می‌شود (مثلاً: علی رضایی ۵۰۰ هزار تومان داد). ثبت پرداخت به مشتری هنوز پشتیبانی نمی‌شود.";

    public const string MultipleAmounts =
        "پیام شما بیش از یک مبلغ دارد. برای جلوگیری از اشتباه، لطفاً هر ثبت را جداگانه بفرستید.";

    public const string UnsupportedRequest =
        "این درخواست در نسخه فعلی پشتیبانی نمی‌شود. فعلاً می‌توانید دریافتی و چک ثبت کنید.";

    public const string ConversationContinuationUnknown =
        "ادامه مکالمه مشخص نیست.";

    public const string CustomerNameRequired =
        "لطفاً نام مشتری را مشخص کنید.";

    public const string CustomerNameEnter =
        "لطفاً نام و در صورت تمایل نام خانوادگی مشتری جدید را وارد کنید.";

    public const string CustomerInformationInvalid =
        "اطلاعات مشتری معتبر نیست.";

    public const string CustomerInformationNotFound =
        "اطلاعات مشتری در مکالمه پیدا نشد.";

    public const string CustomerCreationCancelled =
        "ثبت مشتری لغو شد.";

    public const string CustomerCreationResponseRequired =
        "لطفاً پاسخ دهید که آیا مشتری ثبت شود یا خیر.";

    public const string YesOrNoRequired =
        "لطفاً با «بله» یا «خیر» پاسخ دهید.";

    public const string CustomerSelectionRequired =
        "لطفاً شماره یکی از گزینه‌های زیر را وارد کنید:";

    public const string InvalidCustomerSelection =
        "شماره واردشده معتبر نیست. لطفاً شماره یکی از گزینه‌های زیر را وارد کنید.";

    public const string PreviousActionNotFound =
        "اطلاعات عملیات قبلی پیدا نشد.";

    public const string PaymentAmountRequired =
        "مبلغ پرداخت را مشخص کنید.";

    public const string PaymentAmountNotFound =
        "مبلغ پرداخت در مکالمه پیدا نشد.";

    public const string CheckAmountRequired =
        "مبلغ چک را مشخص کنید.";

    public const string CheckAmountNotFound =
        "مبلغ چک در مکالمه پیدا نشد.";

    public const string CheckDueDateRequired =
        "تاریخ سررسید چک را وارد کنید.";

    public const string InvalidDate =
        "تاریخ معتبر وارد کنید.";

    public const string CheckInformationNotFound =
        "اطلاعات چک در مکالمه پیدا نشد.";

    public const string UnsupportedPendingAction =
        "نوع درخواست قبلی پشتیبانی نمی‌شود.";

    public static string CustomerNotFound(string customerName) =>
        $"مشتری «{customerName}» پیدا نشد. آیا می‌خواهید او را ثبت کنید؟";

    public static string PaymentRegistered(
        decimal amount,
        string customerName) =>
        $"دریافت مبلغ {amount:N0} تومان از {customerName} ثبت شد.";

    public static string CheckRegistered(
        decimal amount,
        string customerName) =>
        $"چک {amount:N0} تومانی برای «{customerName}» با موفقیت ثبت شد.";

    public const string CheckRegistrationQuestion =
        "چک در سامانه صیاد ثبت شده است؟ (بله / خیر)";

    public const string BalanceCustomerAmbiguous =
        "چند مشتری با این نام وجود دارد. نام کامل‌تر را بنویسید.";

    public static string BalanceCustomerNotFound(string name) =>
        $"مشتری «{name}» پیدا نشد.";

    public static string CustomerBalance(
        string customerName,
        decimal cashReceived,
        decimal checksPendingTotal,
        decimal totalReceived) =>
        $"خلاصه دریافتی‌های «{customerName}»:\n" +
        $"نقد دریافتی: {cashReceived:N0} تومان\n" +
        $"چک‌های در انتظار: {checksPendingTotal:N0} تومان\n" +
        $"جمع دریافتی: {totalReceived:N0} تومان\n" +
        "توجه: این عدد فقط مبالغ دریافتی را نشان می‌دهد و بدهی یا فروش در آن لحاظ نشده است.";

    public static string CustomerAndPaymentRegistered(
        string customerName) =>
        $"مشتری «{customerName}» ثبت شد و پرداخت با موفقیت ثبت شد.";

    public static string CustomerAndCheckRegistered(
        string customerName) =>
        $"مشتری «{customerName}» ثبت شد و چک با موفقیت ثبت شد.";
}
