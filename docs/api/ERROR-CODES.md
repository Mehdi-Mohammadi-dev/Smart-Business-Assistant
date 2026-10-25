# کدهای خطا

همه خطاها با `ApiResult` و آرایه `errors` برمی‌گردند: `{ "code": "...", "message": "...", "field": null }`.

| کد | HTTP | معنی | چه کسی اضافه کرد |
|---|---|---|---|
| `PHONE_ALREADY_EXISTS` | 409 | شماره موبایل قبلاً ثبت‌نام کرده | پایه |
| `OTP_INVALID` | 400 | کد تایید نادرست | پایه |
| `OTP_EXPIRED` | 400 | کد تایید منقضی شده | پایه |
| `OTP_TOO_MANY_ATTEMPTS` | 429 | تلاش ناموفق زیاد | پایه |
| `OTP_REQUESTED_TOO_SOON` | 429 | درخواست کد زودتر از موعد | پایه |
| `INVALID_CREDENTIALS` | 401 | موبایل یا رمز نادرست | پایه |
| `INVALID_REFRESH_TOKEN` | 401 | توکن تازه‌سازی نامعتبر | پایه |
| `UNAUTHORIZED` | 401 | ورود لازم است | پایه |
| `SMS_SEND_FAILED` | 502 | ارسال پیامک ناموفق | پایه |
| `VALIDATION_ERROR` | 400 | اعتبارسنجی درخواست (هر فیلد یک خطا) | پایه |
| `INVALID_REQUEST` | 400 | درخواست نامعتبر | پایه |
| `ENTITY_NOT_FOUND` | 404 | موجودیت پیدا نشد (عمومی) | پایه |
| `INVALID_ID` | 400 | شناسه نامعتبر | پایه |
| `INVALID_AMOUNT` | 400 | مبلغ نامعتبر | پایه |
| `INVALID_PHONE_NUMBER` | 400 | شماره موبایل نامعتبر | پایه |
| `INVALID_FIRST_NAME` / `INVALID_LAST_NAME` | 400 | نام یا نام خانوادگی کاربر نامعتبر | پایه |
| `WEAK_PASSWORD` | 400 | رمز ضعیف | پایه |
| `PHONE_NUMBER_REQUIRED` | 400 | موبایل الزامی است | پایه |
| `INVALID_CUSTOMER_FIRST_NAME` / `INVALID_CUSTOMER_LAST_NAME` | 400 | نام مشتری نامعتبر | پایه |
| `INVALID_QUANTITY` / `INVALID_UNIT_PRICE` | 400 | مقدار یا قیمت واحد نامعتبر | پایه |
| `INVALID_SUBSCRIPTION_DATES` | 400 | تاریخ‌های اشتراک نامعتبر | پایه |
| `TOO_MANY_REQUESTS` | 429 | محدودیت نرخ درخواست | پایه |
| `INTERNAL_SERVER_ERROR` | 500 | خطای پیش‌بینی‌نشده | پایه |
| `SUBSCRIPTION_EXPIRED` | 402 | اشتراک تمام شده، ثبت جدید ممکن نیست | صفر |
| `CHECK_NOT_PENDING` | 409 | فقط چک «در انتظار» وصول یا برگشتی می‌شود | صفر (دامین)، ب (استفاده) |
| `CHECK_ALREADY_PENDING` | 409 | چک از قبل «در انتظار» است | صفر (دامین)، ب (استفاده) |
| `CUSTOMER_NOT_FOUND` | 404 | مشتری پیدا نشد یا مال کاربر دیگر است | ب |
| `PAYMENT_NOT_FOUND` | 404 | پرداخت پیدا نشد، باطل‌شده یا مال کاربر دیگر | ب |
| `CHECK_NOT_FOUND` | 404 | چک پیدا نشد، باطل‌شده یا مال کاربر دیگر | ب |
| `STATEMENT_TOO_LARGE` | 400 | بیش از ۵۰۰۰ ردیف در بازه؛ بازه را کوچک‌تر کنید | ب |
| `INVALID_DATE_RANGE` | 400 | تاریخ شروع بعد از تاریخ پایان | ب |

`CUSTOMER_ALREADY_EXISTS` (409) در تسک صفر تعریف شده و هنوز در کد نیست.
