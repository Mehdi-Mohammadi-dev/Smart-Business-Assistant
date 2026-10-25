# API error codes

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
