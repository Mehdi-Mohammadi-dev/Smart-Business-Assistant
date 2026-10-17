using Vexora.Application.NLU.Models.Intents;

namespace Vexora.Application.NLU.Intents;

internal static class IntentKeywords
{
    public static readonly IReadOnlyDictionary<IntentType, string[]> Keywords =
        new Dictionary<IntentType, string[]>
        {
            [IntentType.RegisterPayment] =
            [
                "پرداخت",
                "پرداخت کرد",
                "پرداخت کردم",
                "داد",
                "دادم",
                "واریز",
                "واریز کرد",
                "واریز کردم",
                "گرفت",
                "گرفتم",
                "دریافت",
                "دریافت کرد",
                "دریافت کردم"
            ],

            [IntentType.RegisterSale] =
            [
                "فروخت",
                "فروختم",
                "فروش",
                "جنس داد",
                "جنس دادم",
                "کالا داد",
                "کالا دادم",
                "تحویل داد",
                "تحویل دادم"
            ],

            [IntentType.RegisterDebt] =
            [
                "بدهکار",
                "بدهی",
                "بدهکار شد",
                "بدهکار کردم",
                "قرض"
            ],

            [IntentType.RegisterCheck] =
            [
                "چک",
                "فقره چک",
                "چک گرفتم",
                "چک دریافت کردم",
                "چک دادم",
                "یک چک",
                "چک از"
            ],

            [IntentType.GetCustomerBalance] =
            [
                "چقدر بدهکاره",
                "چقدر بدهکار است",
                "بدهی چقدر است",
                "مانده حساب",
                "حسابش",
                "حساب",
                "بدهکار است"
            ],

            [IntentType.GetCustomerTransactions] =
            [
                "تراکنش",
                "تراکنش ها",
                "سابقه",
                "خریدها",
                "پرداخت ها",
                "گردش حساب"
            ],

            [IntentType.GetCustomerInfo] =
            [
                "اطلاعات",
                "مشخصات",
                "اطلاعات مشتری"
            ],

            [IntentType.GetTodaySales] =
            [
                "فروش امروز",
                "فروش های امروز",
                "امروز چقدر فروختم",
                "امروز چه فروختم"
            ],

            [IntentType.GetTodayPayments] =
            [
                "پرداخت امروز",
                "پرداخت های امروز",
                "امروز چقدر دریافت کردم",
                "امروز چقدر گرفتم"
            ],

            [IntentType.GetUpcomingChecks] =
            [
                "چک های آینده",
                "چک های پیش رو",
                "چک دارم",
                "چک های این هفته",
                "سررسید چک"
            ],

            [IntentType.GetUpcomingReceivables] =
            [
                "مطالبات",
                "دریافتنی",
                "دریافتنی ها",
                "طلب ها",
                "طلب دارم"
            ]
        };
}
