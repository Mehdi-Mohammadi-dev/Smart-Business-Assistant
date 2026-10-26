# جریان ج: پلتفرم، امنیت و انتشار (نفر سوم)

## هدف

برنامه باید **امن، قفل‌شده روی اشتراک، قابل‌انتشار روی سرور** و دارای **خروجی PDF** باشد. مشتری‌ها به این برنامه پول و اعتماد می‌دهند؛ پس چیزهای «معمولی» مثل پشتیبان‌گیری که واقعاً بازگردانی شده، یا محدودیت تعداد درخواست، اینجا کار اصلی هستند.

## پیش‌نیاز: بخوان

1. `README.md`، 2. `CONVENTIONS.md`، 3. `00-task-zero.md` (اتریبیوت اشتراک، DTOهای حساب مشتری)

## چه چیزی را بدون ادغام تسک صفر می‌توانی شروع کنی

همه‌چیز **به‌جز** اتصال PDF به داده واقعی: Docker، امنیت، سلامت، لاگ، تولیدکننده PDF (با داده نمونه)، منطق اشتراک. فقط وقتی `IStatementService` (نفر دوم) ادغام شد، endpoint PDF را وصل کن.

## پوشه‌هایی که مالکشی

```
Vexora.Api/Program.cs   Commons/Middelwares/**   Commons/Filters/**
Vexora.Api/Controllers/Authentications/**   Controllers/Admin/**   Controllers/Reports/**
Vexora.Api/Contracts|Mappers|Validators/ (Authentications, Admin, Reports)
Vexora.Application/Platform/**   (اشتراک، ادمین، قفل ورود)
Vexora.Infrastracture/Pdf/**   Security/**
Dockerfile   docker-compose*.yml   .env.example   deploy/**   .github/workflows/**
Vexora.Application.Test/Platform/**
```

ثبت سرویس‌هایت: `AddPlatformServices` در فایل جدید خودت. **تو مالک `Program.cs` هستی**: دیگران برای تغییر آن، PR کوچک می‌زنند یا از تو می‌خواهند.

## ترتیب پیشنهادی

1. **ج۳ آزمون PDF فارسی** (روز ۱ صبح، چون ریسک فنی آن بالاست)
2. **ج۶ انتشار آزمایشی روی سرور واقعی** (روز ۱ و ۲، چون ناشناخته‌های سرور زمان را می‌خورند)
3. ج۱ قفل اشتراک
4. ج۴ و ج۵ امنیت
5. ج۲ ادمین
6. ج۳ اتصال PDF به داده واقعی

---

## ج۱) قفل اشتراک

**قانون محصول:** بعد از تمام شدن اشتراک، کاربر **همه‌چیز را می‌بیند** (حساب‌ها، لیست‌ها، گفتگوها، PDF) ولی **نمی‌تواند چیزی بنویسد** (پیام به ربات، تأیید ثبت، برگرداندن، تغییر وضعیت چک).

- منطق `[RequireActiveSubscription]` (اسکلت بی‌اثرش در صفر ساخته شده) را بنویس: آخرین اشتراک کاربر (`ISubscriptionRepository.GetLatestByUserIdAsync`) و `Subscription.IsActiveAt(UtcNow)`. اگر اشتراک نبود یا فعال نبود ← `SubscriptionExpiredException` (402، `SUBSCRIPTION_EXPIRED`).
- نتیجه را در همان درخواست فقط یک‌بار بخوان (scoped cache).
- اگر کاربر اشتراکی ندارد (کاربر قدیمی)، مثل منقضی حساب می‌شود.
- فقط روی `POST`/`PUT`/`DELETE` دیگران گذاشته می‌شود؛ تو فقط **منطق** را می‌نویسی و در آخر چک می‌کنی همه endpointهای نوشتن آن را دارند (دستی فهرست کن).
- `GET /api/auth/me` از قبل وضعیت اشتراک را می‌دهد (`isActive`، `remainingDays`)؛ مطمئن شو با منطق جدید ناسازگار نیست.

**تست:** اشتراک فعال، تمام‌شده (نیمه‌شب مرزی با `TimeProvider`)، لغوشده، بدون اشتراک، دو اشتراک (قدیمی تمام‌شده + جدید فعال).

## ج۲) ادمین (تمدید اشتراک دستی)

برای مشتری‌های اولیه، خرید اشتراک با درگاه نداریم؛ **تو** از طریق این endpointها اشتراک را تمدید می‌کنی.

| روش | آدرس | کار |
|---|---|---|
| GET | `/api/admin/users?search=&page=&pageSize=` | نام، موبایل، تاریخ ثبت‌نام، وضعیت اشتراک |
| POST | `/api/admin/users/{id}/subscription` | تمدید یا فعال‌سازی: `{ "days": 30, "planType": "Basic" }` |
| POST | `/api/admin/users/{id}/subscription/cancel` | لغو |

- فقط `UserRole.SystemAdmin` (`[Authorize(Roles = "SystemAdmin")]`؛ نام نقش داخل JWT با claim `role` می‌آید و پروژه آن را `RoleClaimType` گذاشته). کاربر عادی ← `403`.
- **تمدید:** اگر اشتراک فعالی هست، `EndAt` از `max(الان، EndAt فعلی)` به اندازه `days` جلو می‌رود؛ وگرنه اشتراک جدید از الان ساخته و فعال می‌شود. متد دامین `Subscription.Extend(days)` را تو بنویس (با تست).
- `days` بین ۱ و ۳۶۵؛ بیشتر ← `400`.
- اولین ادمین: از تنظیمات (`Admin__Phone`، `Admin__Password`، فقط از متغیر محیطی) هنگام راه‌اندازی ساخته شود **اگر هیچ ادمینی نیست**. رمز در لاگ نیاید.
- هر عملیات ادمین را با شناسه ادمین و کاربر هدف لاگ کن (بدون داده مالی).

## ج۳) PDF حساب مشتری

**خروجی:** `GET /api/customers/{id}/statement?from=&to=` (JSON، مشاهده) و `GET /api/customers/{id}/statement/pdf?from=&to=` (PDF) (کنترلر جدید در `Controllers/Reports`، چون `customers` را نفر دوم دارد).

- `IStatementPdfGenerator.Generate(CustomerStatementDto)` (تعریف در صفر) را در `Vexora.Infrastracture/Pdf` پیاده کن.
- کتابخانه: **QuestPDF** (مجوز Community برای شرکت‌های زیر یک میلیون دلار درآمد سالانه رایگان است؛ شرایط را قبل از استفاده بخوان و در PR بنویس که چک کردی). `QuestPDF.Settings.License = LicenseType.Community`.
- فونت: **وزیرمتن** (مجوز OFL) در `Vexora.Infrastracture/Pdf/Fonts/` (Regular و Bold)، ثبت با `FontManager.RegisterFont`. فایل فونت در گیت می‌ماند.
- جهت صفحه **راست‌به‌چپ**، A4 عمودی.
- محتوا: سربرگ (نام «نوتا»، نام مشتری، بازه، تاریخ تولید شمسی)، جعبه جمع‌ها (دریافتی نقدی، چک‌های در انتظار، وصول‌شده، برگشتی، **جمع دریافتی**)، جدول ردیف‌ها (نوع، تاریخ، مبلغ، وضعیت، سررسید، شماره چک، صیاد)، پاورقی با شماره صفحه («صفحه ۱ از ۳»). سرستون جدول در هر صفحه تکرار شود.
- ارقام **فارسی** و جداکننده هزارگان (`۵۰۰٬۰۰۰`). تاریخ‌ها شمسی (`DatePersian` از DTO).
- **آزمون اول (روز ۱ صبح):** یک PDF نمونه بساز و در یک نمایشگر واقعی (Chrome، Acrobat) باز کن. **حروف فارسی باید به هم چسبیده و درست جهت‌دار باشند** (اگر جدا و برعکس بود، مشکل shaping است). اگر QuestPDF درست نشان نداد، راه جایگزین (تولید HTML و تبدیل به PDF با Chromium بی‌سر) را پیدا و قبل از ادامه با مدیر پروژه هماهنگ کن. این آزمون را قبل از نوشتن بقیه انجام بده.
- پاسخ: `FileContentResult`، `application/pdf`، نام فایل لاتین‌شده امن (`statement-<id>-<yyyyMMdd>.pdf`)، `Content-Disposition` درست. خطاها (مشتری پیدا نشد) به‌صورت `ApiResult` JSON.
- **این endpoint `[RequireActiveSubscription]` ندارد** (خواندن است؛ تصمیم محصول: دانلود PDF بعد از انقضا هم آزاد است).
- سقف: ۵۰۰۰ ردیف (همان خطای `STATEMENT_TOO_LARGE` که سرویس نفر دوم می‌دهد).

**تست:** تولیدکننده برای ۰ ردیف، ۱ ردیف، ۵۰۰ ردیف، نام خیلی بلند، متن ترکیبی فارسی و انگلیسی، مبلغ‌های بزرگ، بدون خطا بایت `%PDF` برگرداند. ظاهر را نمی‌شود خودکار تست کرد، پس یک چک‌لیست دستی (بالا) و یک نمونه خروجی در `docs/samples/` (اختیاری).

## ج۴) سخت‌کردن احراز هویت

### محدودیت تعداد درخواست
الان فقط `send-otp` دارد. اضافه کن (سیاست‌های نام‌دار، مثل موجود):

| سیاست | محدوده | حد پیشنهادی |
|---|---|---|
| `login` | IP | ۱۰ در دقیقه |
| `register` | IP | ۱۰ در دقیقه |
| `refresh` | IP | ۳۰ در دقیقه |
| `chat` | **کاربر** (`userId`) | ۳۰ در دقیقه |

پاسخ ۴۲۹ همان شکل `ApiResult` فعلی را داشته باشد.

### قفل ورود
بعد از ۵ ورود ناموفق در ۱۰ دقیقه برای یک شماره، ورود همان شماره ۱۰ دقیقه قفل شود (`AUTH_TEMPORARILY_LOCKED`، ۴۲۹). شمارنده در Redis (`vexora:login-fail:{phone}`). ورود موفق شمارنده را پاک می‌کند. پیام نباید بگوید «این شماره وجود دارد یا ندارد».

### ارقام فارسی
کاربر ایرانی با کیبورد فارسی می‌نویسد: `۰۹۱۲...` یا کد `۱۲۳۴۵۶`. اضافه کن:

- `PhoneNumberNormalizer` / `DigitNormalizer` در `Application/Common` (ارقام فارسی و عربی ← انگلیسی، حذف فاصله‌ها).
- اعمال روی: شماره موبایل (ثبت‌نام، ورود، ارسال کد، فراموشی رمز) و کد OTP، **قبل** از اعتبارسنجی و پردازش (در Mapper و/یا Validator).
- تست: `۰۹۱۲۳۴۵۶۷۸۹` و `٠٩١٢٣٤٥٦٧٨٩` درست ثبت‌نام و ورود می‌کنند.

### استفاده دوباره از refresh token باطل‌شده
اگر refresh tokenی که قبلاً مصرف (rotate) شده دوباره آمد، نشانه سرقت است: **همه refresh tokenهای آن کاربر باطل شوند**، لاگ امنیتی بنویس (بدون خود توکن) و `401 INVALID_REFRESH_TOKEN` بده. (`RefreshToken.Revoke(replacedBy)` و فیلد `ReplacedByTokenHash` وجود دارد؛ `RevokeAllForUserAsync` را به مخزن اضافه کن.)

### فراموشی رمز
- `POST /api/auth/forgot-password` `{ phoneNumber }` ← کد را (با `OtpPurpose.PasswordReset`) می‌فرستد. **پاسخ همیشه یکسان است**، چه شماره ثبت‌شده باشد چه نه (فاش‌نکردن حساب‌ها).
- `POST /api/auth/reset-password` `{ phoneNumber, otpCode, newPassword }` ← کد را مثل ثبت‌نام بررسی می‌کند (۵ تلاش، انقضا)، رمز را عوض می‌کند و **همه refresh tokenهای کاربر را باطل می‌کند**.
- از همان الگوی OTP و `ISmsSender` فعلی استفاده کن، و محدودیت درخواست مثل `send-otp`.
- قواعد رمز همان ثبت‌نام (حداقل ۸ کاراکتر).

## ج۵) سخت‌کردن API

- **CORS** از تنظیمات (`Cors:AllowedOrigins`، آرایه). `AllowAnyOrigin` در production ممنوع.
- **Swagger** فقط در `Development` یا وقتی `Swagger:Enabled=true`. دکمه **Authorize** (Bearer) را راه بینداز تا بدون Postman تست شود. (نسخه فعلی Swashbuckle با `Microsoft.OpenApi` نسخه ۲ کار می‌کند؛ API متفاوت از آموزش‌های قدیمی است.)
- **هدرهای امنیتی:** `X-Content-Type-Options: nosniff`، `Referrer-Policy: no-referrer`، `X-Frame-Options: DENY`، و `HSTS` در production.
- **Forwarded headers:** پشت پروکسی (`X-Forwarded-For`، `X-Forwarded-Proto`)، وگرنه محدودیت تعداد درخواست همه را یک IP می‌بیند. `KnownProxies` یا `KnownNetworks` را از تنظیمات بخوان.
- **سقف اندازه بدنه** (مثلاً ۶۴ کیلوبایت برای همه به‌جز آپلودها).
- **سلامت:** `GET /health/live` (همیشه ۲۰۰)، `GET /health/ready` (SQL Server و Redis را چک می‌کند). `ready` بدون احراز هویت، ولی فقط وضعیت کلی (بدون جزئیات فنی).
- **لاگ:** Serilog به کنسول (JSON) و فایل چرخشی. هر درخواست `requestId` و `userId` دارد. **هیچ‌وقت متن پیام‌ها، مبلغ‌ها، رمز، توکن یا کد OTP در لاگ نیاید.** (در توسعه، `FakeSmsSender` کد را چاپ می‌کند؛ این فقط در Development باشد.) سطح لاگ از تنظیمات.
- خطای ۵۰۰ فقط پیام عمومی بدهد (وضعیت فعلی درست است؛ چک کن).
- یک سند `docs/api/SECURITY.md` کوتاه: چه چیزی محافظت می‌شود و چگونه.

## ج۶) انتشار (Docker و سرور)

### فایل‌ها

| فایل | محتوا |
|---|---|
| `Dockerfile` | چندمرحله‌ای (`sdk` برای build، `aspnet` برای اجرا)، کاربر غیر root، پورت ۸۰۸۰، `HEALTHCHECK` روی `/health/live` |
| `docker-compose.yml` | سرویس‌ها: `api`، `sqlserver`، `redis`، `proxy` (Caddy برای HTTPS خودکار، یا nginx با گواهی)، `migrator` |
| `.env.example` | همه کلیدها **بدون مقدار واقعی** |
| `deploy/README.md` | راهنمای قدم‌به‌قدم نصب روی سرور خام، به‌روزرسانی، **بازگشت به نسخه قبل** |
| `deploy/backup.sh` | پشتیبان روزانه SQL Server (فشرده، نگه‌داری ۱۴ روز) |
| `deploy/restore.md` | بازگردانی؛ **باید واقعاً یک‌بار تست شود** |
| `deploy/smoke.sh` | بعد از انتشار: `health`، `swagger` خاموش، ثبت‌نام آزمایشی |
| `.github/workflows/release.yml` (اختیاری) | با تگ `v*` ایمیج را build و push کند |

### تنظیمات (همه از متغیر محیطی، بدون هیچ رمز در گیت)

```
ConnectionStrings__Default      ConnectionStrings__Redis      Jwt__Secret
Tsms__Username  Tsms__Password  Tsms__SmsNumber               Sms__Provider=Tsms
Cors__AllowedOrigins__0         Swagger__Enabled=false
Admin__Phone    Admin__Password
```

- رشته اتصال فعلی برای ویندوز است (`Server=.;Trusted_Connection`). برای Docker با کاربر `sa` و رمز از env.
- نام دیتابیس از `VoxoraDb` به `NotaDb`.
- **مایگریشن:** `dotnet ef migrations bundle` (در CI یا داخل ایمیج مایگریتور) و سرویس `migrator` یک‌بار قبل از `api` اجرا شود (`depends_on: condition: service_completed_successfully`). مایگریشن خودکار هنگام بالا آمدن API ممنوع (دو نمونه همزمان خراب می‌کنند).
- **SQL Server:** `mcr.microsoft.com/mssql/server:2022-latest`، حداقل ۲ گیگابایت RAM، `ACCEPT_EULA=Y`، volume ثابت، `healthcheck`.
- **Redis:** `--appendonly yes` و volume، با رمز.
- فقط پروکسی پورت ۸۰ و ۴۴۳ را بیرون می‌دهد؛ `api`، `sqlserver`، `redis` فقط در شبکه داخلی.

### نکته‌های سرور ایران

- ثبت‌های Docker (Docker Hub، `mcr.microsoft.com`، GHCR) ممکن است کند یا مسدود باشند. راه ایمن: ایمیج‌ها را جای دیگر `build` و با `docker save | ssh ... docker load` ببر، یا یک mirror داخلی. این را همان روز ۱ که سرور را می‌گیری امتحان کن.
- پنل پیامک (TSMS) و درگاه‌های ایرانی از سرور داخل ایران پایدارتر هستند. اگر IP محدودیتی (whitelist) دارد، IP سرور را همان اول به پنل بده.
- ساعت سرور اهمیتی ندارد (کد با UTC و جابجایی ایران کار می‌کند)، ولی `TZ` را تنظیم کن تا لاگ‌ها قابل خواندن باشند.

## تست و معیار پذیرش

- [ ] `docker compose up` روی یک ماشین تمیز، بدون هیچ تنظیم دستی دیگر (جز `.env`)، API را بالا می‌آورد. `GET /health/ready` ← ۲۰۰.
- [ ] روی سرور واقعی یک انتشار آزمایشی انجام شده و `deploy/smoke.sh` سبز است.
- [ ] **پشتیبان واقعاً بازگردانی شده** (دیتابیس را پاک کن، از پشتیبان برگردان، ثبت‌ها سر جایشان باشند).
- [ ] بازگشت به نسخه قبل (ایمیج قبلی با تگ) تمرین شده.
- [ ] اشتراک تمام‌شده: نوشتن `402` می‌گیرد، خواندن و PDF `200`.
- [ ] کاربر عادی `403` از `/api/admin/*` می‌گیرد؛ ادمین می‌تواند تمدید کند.
- [ ] محدودیت تعداد درخواست و قفل ورود با تست خودکار.
- [ ] ارقام فارسی در موبایل و OTP.
- [ ] استفاده دوباره از refresh token باطل‌شده ← همه نشست‌ها بسته می‌شوند (تست).
- [ ] فراموشی رمز کامل (پاسخ یکسان برای شماره نامعتبر).
- [ ] PDF فارسی در Chrome و Acrobat درست است (حروف چسبیده).
- [ ] در لاگ هیچ متن پیام، رمز، توکن یا OTP نیست (جست‌وجو کن).
- [ ] Swagger در production خاموش است؛ در Development دکمه Authorize کار می‌کند.
- [ ] `docs/changes/platform.md` نوشته شده.
- [ ] یک نفر دیگر بررسی کرده.

## خارج از محدوده (نساز)

درگاه پرداخت و خرید آنلاین اشتراک، تأیید دو مرحله‌ای، ورود با گوگل، مانیتورینگ پیشرفته (فقط health و لاگ کافی است)، Kubernetes.
