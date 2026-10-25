# کدهای خطای API

هر کس کدی اضافه کرد، همین‌جا ثبت کند. ستون «چه کسی»: جریان (الف، ب، ج، صفر).

| کد | HTTP | معنی | چه کسی |
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
