# جریان ب: دفتر حساب (نفر دوم)

## هدف

کاربر باید بتواند **هر چیزی که ثبت کرده را ببیند و مدیریت کند**: لیست مشتری‌ها و حسابشان، لیست پرداخت‌ها، لیست و وضعیت چک‌ها، و ثبت اشتباه را برگرداند. ثبت‌کردن از مسیر گفتگو انجام می‌شود (جریان الف) و به تو مربوط نیست؛ **تو خواندن و مدیریت را می‌سازی.**

برای فرانت، Swagger تو قرارداد واقعی است. بخش «Swagger» در `CONVENTIONS.md` را جدی بگیر.

## پیش‌نیاز: بخوان

1. `README.md` (قواعد، مالکیت پوشه‌ها)
2. `CONVENTIONS.md` (لایه‌ها، خطاها، Swagger، تست، گیت)
3. `00-task-zero.md` (ستون‌های جدید، متدهای `Check`، DTOهای حساب مشتری که **تعریفشان ثابت است**)

## چه چیزی را بدون ادغام تسک صفر می‌توانی شروع کنی

کوئری‌های خواندن و کنترلرها روی ستون‌های **فعلی**، پیاده‌سازی `IStatementService` (DTOها و رابط از تسک صفر در کد هستند)، seeder. چیزهایی که به ستون‌های جدید (`PaidAt`، `SoftDelete`، `NormalizedName`) نیاز دارند، بعد از ادغام صفر.

## پوشه‌هایی که مالکشی

```
Vexora.Application/Ledger/**              (سرویس‌ها، DTOها، رابط‌های مخزن خواندنی)
Vexora.Api/Controllers/Ledger/**
Vexora.Api/Contracts/Ledger/**   Mappers/Ledger/**   Validators/Ledger/**
Vexora.Infrastracture/.../Repositories/Ledger/**   (کوئری‌های EF)
Vexora.Infrastracture/Seeding/**
Vexora.Application.Test/Ledger/**
```

ثبت سرویس‌هایت: متدهای الحاقی `AddLedgerServices` (Application) و `AddLedgerRepositories` (Infrastracture) در فایل‌های جدید خودت. در دو فایل `DependencyInjection` مشترک فقط **یک خط** برای صدا زدنشان اضافه کن.

**به این‌ها دست نزن:** `Caching/**`، `NLU/**`، `Chats`، `Customer*` (مشتری مال هسته گفتگو است؛ تو فقط می‌خوانی)، `Program.cs`. اگر چیزی لازم داری، بگو.

## قاعده‌های کلی

- همه endpointها `[Authorize]`. **هر کوئری با `UserId` کاربر فعلی فیلتر شود.**
- رکورد حذف‌شده (ابطال‌شده) را فیلتر سراسری EF خودکار کنار می‌گذارد. هیچ‌جا نباید دیده شود، حتی در جمع‌ها.
- همه endpointهای نوشتن (`POST`، `PUT`، `DELETE`) `[RequireActiveSubscription]` دارند. خواندن آزاد است.
- تاریخ «امروز» همیشه به وقت **ایران** (`TimeProvider` + ۳:۳۰). برای تست‌پذیری `TimeProvider` تزریق کن.
- صفحه‌بندی: `page` از ۱، `pageSize` پیش‌فرض ۲۰ و حداکثر ۱۰۰ (بیشتر ← `400`).
- مبلغ‌ها تومان و `decimal`.
- هر تاریخ، یک فیلد شمسی متنی هم دارد (`dueDate` و `dueDatePersian`). تابع تبدیل **از قبل هست** (تسک صفر): `PersianDateFormatter.ToPersian(DateTime)` ← `"1405/09/15"` (ارقام انگلیسی؛ فارسی‌کردن ارقام کار نمایش است).
- `IranTime.Today(TimeProvider)` و `IranTime.Now(TimeProvider)` ← امروز و الان به وقت ایران (از تسک صفر).
- ابطال: `payment.SoftDelete()` یا `check.SoftDelete()` و بعد `IUnitOfWork.SaveAsync`. (متد دامین از تسک صفر.)

## کارها

### ب۱) مشتری‌ها

| روش | آدرس | پاسخ |
|---|---|---|
| GET | `/api/customers?search=&sort=&page=&pageSize=` | `PagedResult<CustomerSummaryResponse>` |
| GET | `/api/customers/{id}` | `CustomerSummaryResponse` |
| GET | `/api/customers/{id}/statement?from=&to=` | `CustomerStatementDto` (شکل در تسک صفر) |

```json
// CustomerSummaryResponse
{
  "id": 12,
  "fullName": "علی رضایی",
  "cashReceived": 3500000,
  "checksPendingTotal": 20000000,
  "checksPendingCount": 2,
  "checksOverdueCount": 1,
  "totalReceived": 5500000,
  "lastActivityAt": "2026-10-24T09:00:00+00:00"
}
```

- `totalReceived = cashReceived + checksPaid` (چک در انتظار و برگشتی داخلش نیست).
- `lastActivityAt`: آخرین ثبت (پرداخت یا چک). مشتری بی‌فعالیت `null` دارد.
- `sort`: `lastActivity` (پیش‌فرض، جدیدترین اول) یا `name`.
- `search`: بخشی از نام کامل. نرمال‌سازی ی/ي و ک/ك و نیم‌فاصله را رعایت کن (بعد از ادغام صفر، روی ستون `NormalizedName`).
- **یک کوئری با `GroupBy`، نه N+1.** برای ۲۰۰ مشتری نباید ۴۰۰ کوئری بخورد.
- مشتری دیگران یا ناموجود ← `404` با کد `CUSTOMER_NOT_FOUND` (استثنای موجود `EntityNotFoundException` هم قابل استفاده است).
- **صورت‌حساب:** `IStatementService` را پیاده کن. سقف ۵۰۰۰ ردیف؛ بیشتر ← `400` با کد `STATEMENT_TOO_LARGE` و پیام «بازه را کوچک‌تر کنید». ترتیب ردیف‌ها و قانون محاسبه دقیقاً طبق تسک صفر. `from > to` ← `400`.
- نفر سوم از `IStatementService` برای PDF استفاده می‌کند. **شکل DTO را تغییر نده.** اگر چیزی کم است، با او هماهنگ کن.

### ب۲) پرداخت‌ها

| روش | آدرس | کار |
|---|---|---|
| GET | `/api/payments?customerId=&from=&to=&page=&pageSize=` | لیست، جدیدترین اول |
| DELETE | `/api/payments/{id}` | ابطال (حذف نرم)، `[RequireActiveSubscription]` |

```json
// PaymentResponse
{
  "id": 91,
  "customerId": 12,
  "customerName": "علی رضایی",
  "amount": 500000,
  "paymentDate": "2026-10-24T09:00:00+00:00",
  "paymentDatePersian": "1405/08/02",
  "direction": "Received"
}
```

- `DELETE`: پرداخت دیگران یا ناموجود یا قبلاً باطل‌شده ← `404` (`PAYMENT_NOT_FOUND`). موفق ← `{ "id": 91 }`.
- ابطال از جمع‌های حساب مشتری و خلاصه حذف می‌شود (فیلتر سراسری).

### ب۳) چک‌ها

| روش | آدرس | کار |
|---|---|---|
| GET | `/api/checks?...` | لیست با فیلتر |
| GET | `/api/checks/{id}` | یک چک |
| POST | `/api/checks/{id}/settle` | وصول شد |
| POST | `/api/checks/{id}/bounce` | برگشت خورد |
| POST | `/api/checks/{id}/revert` | بازگشت به «در انتظار» |
| PUT | `/api/checks/{id}/registration` | ثبت در سامانه صیاد |
| DELETE | `/api/checks/{id}` | ابطال (حذف نرم) |

**فیلترهای لیست:** `customerId`، `settlement` (`Pending`، `Paid`، `Bounced`)، `registration` (`Registered`، `NotRegistered`)، `dueFrom`، `dueTo`، `overdue=true`، `dueWithinDays=N`، `search` (نام مشتری یا شماره چک)، `sort` (`dueDate` پیش‌فرض صعودی، `createdAt`)، `page`، `pageSize`.

- `overdue=true`: فقط `Pending` که `DueDate < امروز(ایران)`.
- `dueWithinDays=N`: فقط `Pending` که `امروز <= DueDate <= امروز+N`.
- فیلترها با AND ترکیب می‌شوند.

```json
// CheckResponse
{
  "id": 40,
  "customerId": 12,
  "customerName": "علی رضایی",
  "amount": 20000000,
  "dueDate": "2026-12-06T00:00:00",
  "dueDatePersian": "1405/09/15",
  "checkNumber": "123456",
  "settlementStatus": "Pending",
  "bankRegistration": "NotRegistered",
  "paidAt": null,
  "bouncedAt": null,
  "isOverdue": false,
  "daysToDue": 43
}
```

- `daysToDue`: برای `Pending`، تعداد روز تا سررسید (منفی یعنی سررسید گذشته)؛ برای بقیه `null`. `isOverdue = Pending && daysToDue < 0`.
- **عملیات** (`settle`، `bounce`، `revert`، `registration`) چک به‌روزشده را برمی‌گردانند. بدنه `settle` و `bounce`: `{ "at": "2026-10-24T09:00:00+00:00" }` اختیاری (پیش‌فرض الان).
- از متدهای دامین `Check` (تسک صفر) استفاده کن. خطاهایشان `AppException` است: `CHECK_NOT_PENDING` (409)، `CHECK_ALREADY_PENDING` (409). **منطق وضعیت را در کنترلر ننویس.**
- چک دیگران یا ناموجود ← `404` (`CHECK_NOT_FOUND`).
- `DELETE`: مثل پرداخت.

### ب۴) خلاصه (داشبورد)

`GET /api/summary` ← 

```json
{
  "today": "2026-10-24",
  "todayPersian": "1405/08/02",
  "customersCount": 18,
  "checks": {
    "pendingCount": 7, "pendingTotal": 85000000,
    "overdueCount": 1, "overdueTotal": 5000000,
    "dueWithin7Count": 2, "dueWithin7Total": 20000000,
    "notRegisteredPendingCount": 4
  },
  "payments": {
    "todayTotal": 1500000,
    "monthTotal": 12000000,
    "monthCount": 9
  }
}
```

- «ماه» یعنی ماه شمسی جاری (از روز ۱ تا امروز) به وقت ایران.
- `todayTotal`: پرداخت‌های امروز ایران.

### ب۵) داده نمونه برای توسعه

یک seeder (فقط `Development`، با `Seed:Enabled=true`) که اگر کاربر نمونه نبود بسازد: کاربر `09120000000`، ۱۵ مشتری (از جمله یک جفت نام نزدیک مثل «علی رضایی» و «علی رضایی بنا»)، ۶۰ پرداخت، ۴۰ چک با ترکیب همه وضعیت‌ها (سررسید گذشته، نزدیک، دور، وصول‌شده، برگشتی، ثبت‌شده و نشده). با یک `HostedService` که فقط در `Development` ثبت می‌شود (نیازی به `Program.cs` نیست). این برای تست دستی همه ما لازم است.

## تست

در `Vexora.Application.Test/Ledger/`:

- محاسبه جمع‌های حساب مشتری (چک برگشتی و در انتظار داخل `TotalReceived` نمی‌آید).
- `isOverdue` و `daysToDue` با `TimeProvider` ثابت، از جمله مرز نیمه‌شب ایران.
- فیلتر `overdue` و `dueWithinDays` (مرزها: امروز، فردا، روز N، روز N+1).
- `PersianDateFormatter` (چند تاریخ معلوم، سال کبیسه، آخر اسفند).
- جداسازی کاربران: سرویس با `UserId` کاربر A هیچ‌وقت داده B را برنمی‌گرداند.
- اگر کوئری EF دارید که منطق دارد، تست با `Microsoft.EntityFrameworkCore.Sqlite` (حافظه) مجاز است؛ پکیج را فقط در پروژه تست اضافه کن.

## تست دستی در Swagger (با seeder)

1. وارد شو (`09120000000`)، توکن را در دکمه Authorize بگذار.
2. `GET /api/summary` ← جمع‌ها با آنچه seeder ساخته می‌خواند.
3. `GET /api/checks?overdue=true` ← فقط سررسید گذشته‌های در انتظار.
4. `POST /api/checks/{id}/settle` ← وضعیت `Paid`، `paidAt` پر؛ دوباره بزن ← `409 CHECK_NOT_PENDING`.
5. `POST /api/checks/{id}/revert` ← `Pending`.
6. `DELETE /api/payments/{id}` ← از لیست و جمع حساب مشتری حذف می‌شود؛ دوباره بزن ← `404`.
7. با یک کاربر دوم لاگین کن و شناسه یک چک کاربر اول را بزن ← `404`.
8. `GET /api/customers/{id}/statement` ← `totals` درست (دستی جمع بزن).

## معیار پذیرش

- [ ] همه endpointهای بالا در Swagger با مثال و کدهای خطا.
- [ ] هیچ کوئری N+1 (لیست مشتری‌ها با ۲۰۰ مشتری، حداکثر چند کوئری ثابت).
- [ ] جداسازی کاربران با تست خودکار ثابت شده.
- [ ] رکورد باطل‌شده در هیچ لیست، جمع و صورت‌حسابی نیست.
- [ ] هیچ `InvalidOperationException` خام به کاربر نمی‌رسد.
- [ ] کدهای خطای جدید در `docs/api/ERROR-CODES.md`.
- [ ] `docs/changes/ledger.md` نوشته شده (چه ساختی، چه تصمیمی، محدودیت‌ها).
- [ ] یک نفر دیگر بررسی کرده.

## خارج از محدوده (نساز)

ویرایش مبلغ یا مشتری یک ثبت (برای ویرایش، ثبت را باطل و دوباره ثبت می‌کنند)، حذف و تغییر نام مشتری، گزارش‌های پیچیده، خروجی Excel، و هر چیزی درباره فروش و بدهی.
