# جریان ب: دفتر حساب

## چه ساخته شد

- `GET /api/customers`، `/api/customers/{id}`، `/api/customers/{id}/statement`
- `GET /api/payments`، `DELETE /api/payments/{id}`
- `GET /api/checks`، `/api/checks/{id}`، `POST settle|bounce|revert`، `PUT registration`، `DELETE`
- `GET /api/summary`
- `IStatementService` با DTOهای تسک صفر (بدون تغییر شکل)، `PagedResult`، `PersianDateFormatter`، `IranTime`
- seeder توسعه (`DevelopmentDataSeeder`)
- تست‌ها در `Vexora.Application.Test/Ledger/`

## ساختار

کوئری‌های EF به‌صورت توابع ایستا روی `IQueryable` در `Application/Ledger/Queries` هستند (فیلتر کاربر، چک‌های سررسید، جمع‌ها). مخزن‌های Infrastructure فقط آن‌ها را اجرا می‌کنند. به همین دلیل تست‌ها بدون دیتابیس، روی `List.AsQueryable()` اجرا می‌شوند.
ثبت سرویس‌ها: `AddLedgerServices` و `AddLedgerRepositories` (یک خط در هر `DependencyInjection` مشترک).

## تصمیم‌ها

- **فقط دریافتی در جمع‌ها:** جمع مشتری، صورت‌حساب و خلاصه فقط `Direction = Received` را می‌شمارند. فهرست پرداخت و چک همه را نشان می‌دهد و فیلد `direction` دارد (فیلد اضافه، سازگار با قرارداد).
- **`lastActivityAt`:** آخرین زمان *ثبت* (`CreateAt`) پرداخت یا چک، نه تاریخ پرداخت.
- **صورت‌حساب:** تاریخ ردیف چک = زمان ثبت، فیلتر بازه روی سررسید. تاریخ‌ها به وقت ایران (+۳:۳۰). جمع‌ها از ردیف‌ها (حداکثر ۵۰۰۰) محاسبه می‌شوند.
- **مشتری‌ها:** سه کوئری ثابت (مشتری‌ها، جمع پرداخت‌ها، جمع چک‌ها)، ادغام و مرتب‌سازی و صفحه‌بندی در حافظه. بدون N+1.
- **جست‌وجو:** تا ادغام ستون `NormalizedName`، نرمال‌سازی (ی/ي، ک/ك، نیم‌فاصله، ارقام) با `REPLACE` روی نام کامل در SQL انجام می‌شود. بعد از ادغام صفر می‌شود روی ستون برد.
- **ترتیب نام:** ترتیب الفبای فارسی (ا ب پ ... ی) در حافظه.
- **`sort`:** مقدارها `LastActivity`/`Name` و `DueDate`/`CreatedAt` هستند (حروف بزرگ و کوچک مهم نیست). `CreatedAt` جدیدترین اول.
- **`PUT registration`:** بدنه `{ "bankRegistration": "Registered" }`، برگشت به `NotRegistered` هم ممکن است.
- **خطاهای چک:** `CheckNotPending/AlreadyPending` در Domain هستند (Domain به Application وابسته نیست) و `GlobalExceptionHandlingMiddleware` آن‌ها را به 409 با کدهای `CHECK_NOT_PENDING` و `CHECK_ALREADY_PENDING` نگاشت می‌کند.

## بخش‌هایی از صفر که برای کامپایل لازم بود (باید با نسخه رسمی صفر یکی شود)

نیاز به مایگریشن: `DeletedAt` (همه موجودیت‌ها)، `Check.PaidAt` و `Check.BouncedAt`.

- `BaseEntity` (`DeletedAt`، `SoftDelete()`) و `BaseEntityConfiguration`
- `Check` (`PaidAt`، `BouncedAt`، `MarkAsPaid(at)`، `MarkAsBounced(at)`، `RevertToPending`، `SetBankRegistration`) و `CheckConfiguration`، `CheckExceptions`
- `AppErrorType` (402 و 403)، `SubscriptionExpiredException`، نگاشت در middleware
- `[RequireActiveSubscription]` (بی‌اثر)، `JsonStringEnumConverter` در `Program.cs`
- `PagedResult`، DTOهای صورت‌حساب، `IStatementService`، `IStatementPdfGenerator`
- `docs/api/ERROR-CODES.md`

مایگریشن ساخته نشده است (قرارداد: فقط مدیر پروژه).

## seeder

```
dotnet user-secrets set "Seed:Enabled" "true" --project Vexora.Api
```
فقط در `Development` اجرا می‌شود. کاربر `09120000000` با رمز `Seed@12345`، ۱۵ مشتری، ۶۰ پرداخت، ۴۰ چک. اگر کاربر وجود داشته باشد کاری نمی‌کند.

## محدودیت‌ها

- نمونه (`example`) مقدارها در Swagger نیست، فقط توضیح و کدهای خطا. (کامنت XML ممنوع است و فیلتر اسکیما ساخته نشد.)
- `[RequireActiveSubscription]` تا پیاده‌سازی نفر سوم اثری ندارد.
- ایندکس ترکیبی برای فهرست چک‌ها (`UserId, SettlementStatus, DueDate`) اضافه نشده؛ در صورت کندی بعد از مایگریشن اضافه شود.
- هیچ build یا تست اجرا نشده است (این محیط .NET ندارد).
