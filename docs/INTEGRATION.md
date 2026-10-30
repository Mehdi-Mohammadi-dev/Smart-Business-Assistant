# راهنمای اتصال فرانت‌اند

همه پاسخ‌های API در قالب عمومی پروژه برگردانده می‌شوند: `{ success, message, data, errors[] }`. برای endpointهای نیازمند احراز هویت، هدر `Authorization: Bearer <token>` را ارسال کنید.

## گردش حساب مشتری
- نمایش JSON: `GET /api/customers/{id}/statement?from=2026-01-01&to=2026-03-31`
- خروجی PDF: `GET /api/customers/{id}/statement/pdf?from=...&to=...`
- `from` و `to` اختیاری‌اند (میلادی، `yyyy-MM-dd`). در صورت حذف، کل بازه استفاده می‌شود. اگر `from > to` باشد، پاسخ 400 است.
- PDF را با `responseType: blob` دریافت کنید. بیش از ۵۰۰۰ ردیف خطا می‌دهد؛ بازه را کوچک‌تر کنید.
- نام کسب‌وکار در سربرگ PDF می‌آید. گزارش فعلی دریافتی‌ها را نمایش می‌دهد و بدهی را محاسبه/نمایش نمی‌دهد.

## پلن‌ها و اشتراک
- فهرست عمومی پلن‌ها: `GET /api/plans` (بدون توکن)؛ شامل `id`, `name`, `days`, `priceToman`, `discountPercent`, `finalPriceToman`, `pricePerDayToman`, `isFeatured`.
- مدیریت پلن‌ها برای نقش `SystemAdmin`: `GET/POST /api/admin/plans`, `PUT/DELETE /api/admin/plans/{id}`.
- بدنهٔ ایجاد/ویرایش پلن: `{ name, days, priceToman, discountPercent, isActive, isFeatured, sortOrder }`.
- قیمت را از سرور دریافت کنید؛ مبلغ را به‌عنوان قیمت نهایی از کلاینت قابل اعتماد ندانید. واحد مبالغ تومان است.
- عملیات نوشتنی مشخص‌شده با `[RequireActiveSubscription]` برای کاربران عادی به اشتراک فعال نیاز دارند. درخواست‌های `GET`, `HEAD`, `OPTIONS` از این فیلتر عبور می‌کنند.
- نقش `SystemAdmin` از فیلتر اشتراک مستثناست؛ مدیر برای اجرای عملیات مدیریتی یا عبور از این فیلتر به خرید اشتراک نیاز ندارد.

## معرفی دوستان (Referral)

### کاربر عادی: فهرست دعوت‌شدگان من
- Endpoint: `GET /api/referrals/mine?page=1&pageSize=20`
- نیازمند توکن معتبر است. داده‌ها فقط مربوط به افرادی هستند که با کد دعوت کاربر جاری ثبت‌نام کرده‌اند؛ شناسهٔ معرف از هویت احراز‌شده گرفته می‌شود، نه از پارامتر ارسالی کلاینت.
- پارامترها: `page` از ۱ به بالا؛ `pageSize` بین ۱ تا ۱۰۰ (پیش‌فرض ۲۰).
- فیلدهای هر ردیف:
  - `referredUserId`, `referredName`, `referredPhoneNumber`: اطلاعات کاربر دعوت‌شده.
  - `registeredAt`: زمان ثبت‌نام.
  - `hasPurchasedSubscription`: آیا برای کاربر دعوت‌شده اشتراک خریداری‌شده‌ای ثبت شده است؛ اشتراک آزمایشی/رایگان به‌تنهایی خرید محسوب نمی‌شود.
  - `subscriptionActive`: آیا آخرین اشتراک خریداری‌شده در حال حاضر فعال است.
  - `subscriptionPlanType`: نوع پلن اشتراک خریداری‌شده، در صورت وجود.
  - `subscriptionEndAt`: زمان پایان اشتراک، در صورت وجود.

### مدیر سیستم: گزارش دعوت‌ها
- Endpoint: `GET /api/admin/referrals?page=1&pageSize=50`
- فقط نقش `SystemAdmin` مجاز است. پارامترهای صفحه‌بندی همان محدودیت بالا را دارند؛ پیش‌فرض `pageSize` برابر ۵۰ است.
- علاوه بر اطلاعات معرف و کاربر دعوت‌شده، فیلدهای `referrerUserId`, `referrerName`, `referrerPhoneNumber`, `referrerInviteCode`, `totalReferredByReferrer`, `rewardGranted`, `rewardDays` و وضعیت اشتراک (`hasPurchasedSubscription`, `subscriptionActive`, `subscriptionPlanType`, `subscriptionEndAt`) برگردانده می‌شوند.
- `totalReferredByReferrer` تعداد ثبت‌نام‌های معرفی‌شده توسط آن معرف است.

### تنظیمات طرح معرفی (مدیر سیستم)
- مشاهده: `GET /api/admin/referrals/settings`
- تغییر: `PUT /api/admin/referrals/settings`
- فقط نقش `SystemAdmin` مجاز است.
- بدنهٔ درخواست: `{ "isEnabled": true, "rewardDays": 7, "maxRewardDaysPerUser": 30 }` (مقادیر نمونه‌اند؛ مقدارهای واقعی را مطابق سیاست محصول انتخاب کنید).
- `rewardDays` و `maxRewardDaysPerUser` نباید منفی باشند. اگر تنظیمی هنوز ثبت نشده باشد، مقدارهای پیش‌فرض پاسخ به‌ترتیب `false`, `0`, `0` هستند.

## ثبت‌نام
- `BusinessName` در ثبت‌نام اختیاری است.
- وضعیت خرید/فعال بودن اشتراک در گزارش Referral از رکورد اشتراک خریداری‌شده استخراج می‌شود؛ ثبت‌نام دعوت‌شده به‌تنهایی به معنی خرید اشتراک نیست.

## خطاهای متداول
- `400`: پارامترهای نامعتبر یا درخواست نامعتبر.
- `401`: توکن نامعتبر/منقضی یا احراز هویت انجام نشده.
- `403`: کاربر احراز هویت شده اما نقش لازم (برای مثال `SystemAdmin`) را ندارد.
