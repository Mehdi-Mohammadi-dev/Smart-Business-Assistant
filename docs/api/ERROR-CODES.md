# کدهای خطا

همه خطاها با `ApiResult` و آرایه `errors` برمی‌گردند: `{ "code": "...", "message": "...", "field": null }`.

| کد | HTTP | معنی | چه کسی اضافه کرد |
|---|---|---|---|
| `PHONE_ALREADY_EXISTS` | 409 | این شماره قبلاً ثبت‌نام کرده | قبلی |
| `OTP_INVALID` | 400 | کد تأیید نادرست است | قبلی |
| `OTP_EXPIRED` | 400 | کد منقضی شده | قبلی |
| `OTP_TOO_MANY_ATTEMPTS` | 429 | تلاش ناموفق زیاد | قبلی |
| `OTP_REQUESTED_TOO_SOON` | 429 | کد جدید خیلی زود | قبلی |
| `INVALID_CREDENTIALS` | 401 | شماره یا رمز نادرست | قبلی |
| `INVALID_REFRESH_TOKEN` | 401 | نشست نامعتبر | قبلی |
| `UNAUTHORIZED` | 401 | وارد نشده | قبلی |
| `SMS_SEND_FAILED` | 502 | پیامک ارسال نشد | قبلی |
| `TOO_MANY_REQUESTS` | 429 | محدودیت تعداد درخواست | قبلی |
| `SUBSCRIPTION_EXPIRED` | 402 | اشتراک تمام شده (فقط خواندن) | صفر |
| `CUSTOMER_ALREADY_EXISTS` | 409 | مشتری هم‌نام وجود دارد | صفر |
| `CHECK_NOT_PENDING` | 409 | چک در انتظار نیست | صفر |
| `CHECK_ALREADY_PENDING` | 409 | چک از قبل در انتظار است | صفر |
| `CUSTOMER_NOT_FOUND` | 404 | مشتری پیدا نشد | ب |
| `PAYMENT_NOT_FOUND` | 404 | پرداخت پیدا نشد | ب |
| `CHECK_NOT_FOUND` | 404 | چک پیدا نشد | ب |
| `STATEMENT_TOO_LARGE` | 400 | صورت‌حساب بیش از ۵۰۰۰ ردیف | ب |
| `AUTH_TEMPORARILY_LOCKED` | 429 | ورود موقتاً قفل شد | ج |
| `CUSTOMER_NAME_TOO_LONG` | 400 | نام یا نام خانوادگی بیش از ۸۰ کاراکتر | صفر |
| `CARD_NOT_PENDING` | 409 | کارت دیگر قابل تأیید یا لغو نیست | صفر |
| `CONVERSATION_NOT_FOUND` | 404 | گفتگو پیدا نشد (یا مال کاربر دیگر) | صفر |
| `MESSAGE_NOT_FOUND` | 404 | پیام پیدا نشد | صفر |
| Code | HTTP | Meaning | Added by |
|---|---:|---|---|
| `PHONE_ALREADY_EXISTS` | 409 | شماره موبایل قبلاً ثبت شده است | Existing |
| `OTP_INVALID` | 400 | کد تأیید نامعتبر است | Existing |
| `OTP_EXPIRED` | 400 | کد تأیید منقضی شده است | Existing |
| `OTP_TOO_MANY_ATTEMPTS` | 429 | تعداد تلاش‌های کد بیش از حد است | Existing |
| `OTP_REQUESTED_TOO_SOON` | 429 | درخواست کد زودتر از موعد است | Existing |
| `INVALID_CREDENTIALS` | 401 | اطلاعات ورود نامعتبر است | Existing |
| `INVALID_REFRESH_TOKEN` | 401 | refresh token نامعتبر یا باطل شده است | Platform |
| `AUTH_TEMPORARILY_LOCKED` | 429 | ورود برای این شماره موقتاً قفل شده است | Platform |
| `WEAK_PASSWORD` | 400 | رمز عبور کمتر از ۸ نویسه است | Platform |
| `SUBSCRIPTION_EXPIRED` | 402 | اشتراک فعال وجود ندارد | Platform |
| `FORBIDDEN` | 403 | اجازه انجام عملیات وجود ندارد | Platform |
| `TOO_MANY_REQUESTS` | 429 | محدودیت درخواست اعمال شده است | Platform |
| `REQUEST_TOO_LARGE` | 413 | بدنه درخواست از ۶۴ کیلوبایت بیشتر است | Platform |
| `STATEMENT_TOO_LARGE` | 400 | گزارش بیش از ۵۰۰۰ ردیف دارد | Platform |
| `INTERNAL_SERVER_ERROR` | 500 | خطای عمومی سرور | Existing |
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
