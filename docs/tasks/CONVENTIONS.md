# قراردادهای کدنویسی و همکاری

## ساختار پروژه (لایه‌ها)

```
Vexora.Domain          قانون‌های کسب‌وکار (موجودیت‌ها، enum، اعتبارسنجی). به هیچ‌جا وابسته نیست
Vexora.Application     منطق برنامه (سرویس‌ها، DTO، رابط مخزن‌ها). فقط به Domain وابسته است
Vexora.Infrastracture  دنیای بیرون (EF، Redis، SMS، PDF). به Application وابسته است
Vexora.Api             کنترلرها، قراردادهای HTTP، اعتبارسنجی. وابستگی‌ها را وصل می‌کند
Vexora.Application.Test  تست‌ها
```

قانون طلایی: **وابستگی فقط به سمت داخل.** Application هیچ‌وقت چیزی از EF، Redis، QuestPDF یا ASP.NET نمی‌شناسد. رابط (interface) در Application، پیاده‌سازی در Infrastracture.

(نام پروژه‌ها غلط املایی دارد، `Infrastracture` و `Percistance`. فعلاً همین است و بعد از ادغام اول درست می‌شود.)

## الگوی یک endpoint

```
Api/Controllers/<گروه>/XController.cs   ← فقط درخواست را می‌گیرد و جواب را برمی‌گرداند
Api/Contracts/<گروه>/XRequest.cs, XResponse.cs   ← شکل JSON
Api/Mappers/<گروه>/XMapper.cs            ← تبدیل Contract ↔ DTO
Api/Validators/<گروه>/XValidator.cs      ← FluentValidation روی Request
Application/Services/...                  ← منطق
Application/Dtos/...                      ← داده بین لایه‌ها
Application/Repositories/...              ← رابط مخزن
Infrastracture/.../Repositories/...       ← کوئری EF
```

- پاسخ همه endpointها (جز فایل) با `ApiResult<T>` است:
  `{ "success": true, "message": "...", "data": {...}, "errors": [] }`
- اعتبارسنجی درخواست را `ValidationFilter` خودکار انجام می‌دهد: برای هر نوع بدنه، `IValidator<T>` در Api پیدا و اجرا می‌شود.
- هر کنترلر `[Authorize]` دارد (جز ثبت‌نام و ورود).
- کاربر فعلی: `ICurrentUser.UserId` (از JWT). **هر کوئری روی داده کاربر حتماً با `UserId` فیلتر شود.** هیچ‌وقت شناسه کاربر را از بدنه یا آدرس نگیر.
- حذف‌شده‌ها (`IsDelete = true`) را فیلتر سراسری EF خودکار کنار می‌گذارد؛ نیازی به `Where(!IsDelete)` نیست. اگر باید رکورد حذف‌شده را ببینی از `IgnoreQueryFilters()` استفاده کن و دلیلش را در PR بنویس.

## خطاها (خیلی مهم)

- خطای مورد انتظار (پیدا نشد، تکراری، اشتراک تمام شد، ...) را با کلاسی بساز که از `AppException` ارث می‌برد (`Vexora.Domain/sharedKernel/Exceptions/AppException.cs`، در Domain تا استثناهای دامین هم بتوانند از آن ارث ببرند). `Type` آن تعیین می‌کند چه کد HTTP برگردد:

| `AppErrorType` | کد HTTP |
|---|---|
| Validation | 400 |
| Unauthorized | 401 |
| PaymentRequired | 402 |
| Forbidden | 403 |
| NotFound | 404 |
| Conflict | 409 |
| TooManyRequests | 429 |
| ExternalService | 502 |

- هر استثنا یک **کد ثابت انگلیسی** (مثلاً `CHECK_ALREADY_SETTLED`) و **پیام فارسی قابل نمایش** دارد. پیام هرگز جزئیات فنی (نام جدول، رشته SQL) را نشان نمی‌دهد.
- **هرگز `InvalidOperationException` خام یا `Exception` پرتاب نکن** برای چیزی که کاربر می‌تواند باعثش شود. خروجی‌اش ۵۰۰ می‌شود.
- هر کد خطای جدید را در `docs/api/ERROR-CODES.md` (ستون: کد، HTTP، معنی، چه کسی اضافه کرد) بنویس.

## Swagger = قرارداد (چون فرانت از روی آن ساخته می‌شود)

هر endpoint باید داشته باشد:

- `[EndpointSummary("...")]` و `[EndpointDescription("...")]` فارسی و کوتاه
- `[ProducesResponseType<ApiResult<X>>(200)]` و همه کدهای خطای ممکن (`400`، `401`، `402`، `404`، `409`)
- اسم‌های JSON در camelCase، تاریخ ISO-8601، مبلغ `decimal` به **تومان**
- enumها در JSON به‌صورت **رشته** (مثلاً `"Pending"`)، نه عدد. (تنظیم سراسری در صفر انجام می‌شود)
- هر تاریخی که کاربر می‌بیند، علاوه بر نسخه ISO، یک نسخه شمسی متنی داشته باشد: `dueDate` و `dueDatePersian` (`"1405/09/15"`)

چون کامنت XML ممنوع است، توضیحات را فقط با اتریبیوت‌ها بنویس.

## دیتابیس و مایگریشن

- مایگریشن‌ها بین شاخه‌ها تداخل می‌سازند (فایل `ModelSnapshot`). قاعده:
  1. **فقط مدیر پروژه** مایگریشن می‌سازد، و فقط روی آخرین `main`.
  2. تغییر ستون یا جدول جدید لازم داری؟ در PR فقط **کلاس‌ها و پیکربندی EF** را بنویس و در توضیح PR بنویس «نیاز به مایگریشن». مدیر پروژه بعد از ادغام، مایگریشن را می‌سازد.
  3. اگر مجبور شدی محلی مایگریشن بسازی تا تست کنی، **قبل از PR پاکش کن** (`dotnet ef migrations remove`).
- مبلغ: `decimal`، همیشه تومان. تاریخ‌های لحظه‌ای: `DateTimeOffset` و UTC. تاریخ سررسید چک: `DateTime` (فقط روز، به وقت ایران).
- هیچ‌وقت داده را فیزیکی حذف نکن (`Remove`)؛ از `SoftDelete()` استفاده کن.

## تست

- فریمورک: xUnit + FluentAssertions (+ Moq). تست‌ها را در **پوشه مخصوص جریان خودت** بنویس: `Vexora.Application.Test/Ledger/**`، `.../Platform/**`، `.../Conversations/**`. فایل تست مشترک ممنوع.
- تست‌ها **خودکفا** و بدون دیتابیس واقعی و بدون ساعت واقعی باشند: به‌جای `DateTime.Now`، `TimeProvider` تزریق کن (نمونه: `FixedTimeProvider` در تست‌ها).
- برای هر قانون کسب‌وکار یک تست که اگر قانون خراب شود شکست بخورد.
- تست دستی: هر فایل تسک یک بخش «تست دستی در Swagger» دارد. قبل از PR انجامش بده و نتیجه را در توضیح PR بنویس.

## قالب کد

- بدون کامنت. نام‌های روشن به‌جای توضیح.
- سازنده اصلی (primary constructor) برای تزریق وابستگی، مثل بقیه پروژه.
- `async` با `CancellationToken` در همه متدهای ورودی/خروجی.
- فایل‌های جدید: هر نوع در فایل خودش (به‌جز ریکوردهای کوچک و مرتبط DTO).
- پیام‌های فارسی کاربر نهایی در `Constants/Messages/...` یا خود کلاس استثنا، نه پراکنده در کد.

## گیت

- شاخه: `feature/<جریان>-<موضوع>` (مثلاً `feature/ledger-check-actions`).
- کامیت: `نوع(حوزه): توضیح کوتاه انگلیسی` (`feat(ledger): add check settle endpoint`).
- PR کوچک (کمتر از حدود ۴۰۰ خط تغییر) ارجح است. PR بزرگ را به چند PR بشکن.
- قبل از PR: `git pull --rebase origin main`.

## امنیت (همه)

- رمز و کلید هرگز در گیت نیست: `user-secrets` در توسعه، متغیر محیطی در سرور.
- متن پیام کاربر و مبلغ‌ها داده مالی هستند: در لاگ **نیاید** (فقط شناسه‌ها و کدها).
- هر ورودی کاربر را اعتبارسنجی کن (طول، قالب، محدوده).
