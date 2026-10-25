# تسک صفر: پایه دیتابیس و قراردادهای مشترک

**وضعیت: کد نوشته و آزمایش شده (مرحله Domain و Application). منتظر build و مایگریشن توسط مدیر پروژه.**

**چرا اول؟** سه نفر روی یک دیتابیس کار می‌کنند. اگر هر کس ستون خودش را اضافه کند، مایگریشن‌ها تداخل می‌کنند. پس همه تغییرات دیتابیس نسخه ۱ و همه رابط‌ها و DTOهای مشترک در یک PR اول آمدند.

## ۱) چه چیزی ساخته شد

### دامین

| مورد | تغییر |
|---|---|
| `BaseEntity` | ستون `DeletedAt` و متد `SoftDelete()` (دوبار صدا زدن بی‌اثر است). **توجه:** `ApplicationDbContext` از قبل `Remove()` را هم به حذف نرم تبدیل می‌کرد (و حالا `DeletedAt` را هم پر می‌کند). هر دو راه درست است؛ ترجیح ما `entity.SoftDelete()` است چون صریح است |
| `Customer` | ستون `NormalizedName`، ویژگی `FullName`، نرمال‌سازی نام (`CustomerNameNormalizer`: ی/ي، ک/ك، نیم‌فاصله، فاصله‌های اضافه، اعراب، ارقام، حروف لاتین کوچک)، سقف ۸۰ کاراکتر برای نام و فامیل (`CUSTOMER_NAME_TOO_LONG`) |
| `Check` | ستون‌های `PaidAt` و `BouncedAt`؛ متدهای `MarkAsPaid(at)`، `MarkAsBounced(at)`، `RevertToPending()`، `SetBankRegistration(status)` (جایگزین `MarkAsRegistered`). خطا: `CHECK_NOT_PENDING` و `CHECK_ALREADY_PENDING` (409، نه `InvalidOperationException`) |
| `Conversation` و `ConversationMessage` | موجودیت‌های جدید گفتگو (کلید `Guid`، حذف نرم برای گفتگو، `CardState` برای کارت تأیید). مسیر کار روی آن‌ها در `01-core-conversation.md` است |
| `AppException` و `AppErrorType` | **از Application به Domain منتقل شد** (`Vexora.Domain.sharedKernel.Exceptions`) تا استثناهای دامین هم بتوانند از آن ارث ببرند. دو نوع جدید: `PaymentRequired` (402)، `Forbidden` (403) |
| `SubscriptionExpiredException` | در `SubscriptionExceptions` (کد `SUBSCRIPTION_EXPIRED`، 402) |

### Application

| مورد | تغییر |
|---|---|
| `Application/Common/IranTime.cs` | `Today(TimeProvider)` و `Now(TimeProvider)` به وقت ایران (UTC+3:30، ایران ساعت تابستانی ندارد) |
| `Application/Common/PersianDateFormatter.cs` | `ToPersian(DateTime)` و `ToPersian(DateTimeOffset)` ← `"1405/09/15"` (ارقام انگلیسی) |
| `Application/Ledger/Dtos/**` | `PagedResult<T>`، `StatementTotalsDto`، `StatementRowDto`، `CustomerStatementDto` (شکل **ثابت** است، در پایین) |
| `Application/Ledger/Services/**` | `IStatementService` (نفر دوم پیاده می‌کند)، `IStatementPdfGenerator` (نفر سوم پیاده می‌کند) |
| `CustomerService` | جستجو و تطبیق با `NormalizedName`. **«نام دقیق» برنده است** (اگر «علی رضایی» و «علی رضایی بنا» هر دو هستند، نوشتن «علی رضایی» همان اولی را برمی‌گرداند). `CreateAsync` برای نام تکراری (حتی با ي/ی متفاوت) مشتری موجود را برمی‌گرداند و چیزی نمی‌سازد |
| `ICustomerRepository` | `GetByExactNameAsync` ← `GetByNormalizedNameAsync`؛ `SearchByNameAsync` پارامتر نرمال‌شده می‌گیرد |

### Infrastracture

| مورد | تغییر |
|---|---|
| ایندکس مشتری | ایندکس یکتای قدیمی (`UserId, FirstName, LastName`) حذف و **ایندکس یکتای فیلترشده** `IX_Customers_UserId_NormalizedName` روی (`UserId`, `NormalizedName`) با شرط `IsDelete = 0` اضافه شد. نام مشتری حذف‌شده دوباره قابل استفاده است |
| `UnitOfWork.SaveAsync` | نقض یکتایی آن ایندکس (خطای SQL 2601 یا 2627) را به `CUSTOMER_ALREADY_EXISTS` (409) تبدیل می‌کند (برای وقتی که دو درخواست همزمان یک نام را بسازند) |
| ایندکس‌های لیست | `Checks(UserId, SettlementStatus, DueDate)` و `Payments(UserId, PaymentDate)` برای سرعت لیست‌های جریان ب |
| جدول‌ها | `Conversations` و `ConversationMessages` با تبدیل enum به رشته و فیلتر حذف نرم |

### Api

- `[RequireActiveSubscription]` (اسکلت بی‌اثر): نفر سوم منطقش را می‌نویسد.
- نگاشت `PaymentRequired` ← 402 و `Forbidden` ← 403 در `GlobalExceptionHandlingMiddleware`.
- enumها در JSON به‌صورت **رشته** (`JsonStringEnumConverter`).

## ۲) قرارداد حساب مشتری (بین ب و ج؛ بدون هماهنگی تغییر نمی‌کند)

(کد در `Application/Ledger/Dtos/CustomerStatementDto.cs`.)

```csharp
public sealed record StatementTotalsDto(
    decimal CashReceived, decimal ChecksPending, decimal ChecksPaid, decimal ChecksBounced,
    decimal TotalReceived, int PaymentsCount, int ChecksCount);

public sealed record StatementRowDto(
    string Type, long Id, DateTime Date, string DatePersian, decimal Amount,
    string? Status, string? CheckNumber, DateTime? DueDate, string? DueDatePersian,
    string? BankRegistration);

public sealed record CustomerStatementDto(
    long CustomerId, string CustomerName, DateTime? From, DateTime? To,
    StatementTotalsDto Totals, IReadOnlyList<StatementRowDto> Rows, DateTimeOffset GeneratedAt);
```

- `Type` یکی از `"Payment"` یا `"Check"`.
- پرداخت: `Date` = تاریخ پرداخت، `Status` = `null`.
- چک: `Date` = تاریخ ثبت، `DueDate` = سررسید، `Status` = `"Pending"`/`"Paid"`/`"Bounced"`، `BankRegistration` = `"Registered"`/`"NotRegistered"`.
- **`TotalReceived = CashReceived + ChecksPaid`.** چک در انتظار و برگشتی داخلش نیست.
- ردیف‌ها بر اساس `Date` از جدید به قدیم. ابطال‌شده‌ها هیچ‌جا نمی‌آیند.
- بازه `From`/`To`: برای پرداخت روی `Date`، برای چک روی `DueDate`. هر دو اختیاری.
- «مانده حساب» عمداً وجود ندارد (نسخه ۱ فروش و بدهی ندارد).

## ۳) مایگریشن (فقط مدیر پروژه)

```powershell
git checkout main; git pull
git checkout -b feature/zero-schema
# فایل‌های بسته را روی پروژه بگذار، بعد:
dotnet build
dotnet test
cd Vexora.Api
dotnet ef migrations add ZeroSchemaAndConversations --project ..\Vexora.Infrastracture --startup-project .
```

### ⚠ قبل از `database update` فایل مایگریشن ساخته‌شده را ویرایش کن

مایگریشن، ستون `NormalizedName` را با مقدار خالی به همه مشتری‌های قدیمی می‌دهد و **بعد** ایندکس یکتا می‌سازد. اگر یک کاربر بیش از یک مشتری داشته باشد، همه `NormalizedName` خالی و برابرند و ساخت ایندکس شکست می‌خورد. پس در متد `Up` همان فایل، **بین** `AddColumn` مربوط به `NormalizedName` و `CreateIndex` مربوط به `IX_Customers_UserId_NormalizedName` این را بگذار:

```csharp
migrationBuilder.Sql(@"
UPDATE [Customers]
SET [NormalizedName] = LOWER(LTRIM(RTRIM(
    REPLACE(REPLACE(CONCAT([FirstName], N' ', [LastName]), NCHAR(1610), NCHAR(1740)), NCHAR(1603), NCHAR(1705)))))");
```

(`NCHAR(1610)` ← `ي` و `NCHAR(1740)` ← `ی`؛ `NCHAR(1603)` ← `ك` و `NCHAR(1705)` ← `ک`.)

اگر بعد از آن، `database update` به‌خاطر نام تکراری شکست خورد، یعنی دو مشتری واقعاً هم‌نام (فقط با ي/ی متفاوت) هستند. پیدایشان کن:

```sql
SELECT UserId, NormalizedName, COUNT(*) AS Cnt
FROM Customers WHERE IsDelete = 0
GROUP BY UserId, NormalizedName HAVING COUNT(*) > 1;
```

و یکی را تغییر نام بده (مثلاً «علی رضایی بنا»)، بعد دوباره `database update`.

```powershell
dotnet ef database update --project ..\Vexora.Infrastracture --startup-project .
```

بعد PR، ادغام، و اعلام به بقیه: «`main` را pull کنید و دیتابیس را update کنید».

## ۴) معیار پذیرش

- [x] ۵۸ تست جدید و همه تست‌های NLU قبلی (۳۱۲ تست) سبز (در اجراکننده‌ای که Domain و Application را کامپایل می‌کند).
- [ ] `dotnet build` و `dotnet test` کامل (شامل Infrastracture، Api و تست‌های Moq) سبز، روی ماشین مدیر پروژه.
- [ ] مایگریشن روی دیتابیس **خالی** و روی دیتابیس فعلی اجرا می‌شود.
- [ ] یک مشتری هم‌نام برای همان کاربر (حتی با «ي» به‌جای «ی»): دیتابیس رد می‌کند و API ۴۰۹ `CUSTOMER_ALREADY_EXISTS` می‌دهد. نام مشتری حذف‌شده دوباره قابل ساخت است.
- [ ] `payment.SoftDelete()` و ذخیره: رکورد از کوئری‌های عادی حذف می‌شود و `DeletedAt` پر است.
- [ ] Swagger باز می‌شود، enumها رشته‌اند، DTOهای `Ledger` دیده می‌شوند.
- [ ] هیچ endpointی خراب نشده (ثبت‌نام، ورود، چت).
