# تغییرات Referral و دسترسی مدیر

## قابلیت‌های API

### فهرست دعوت‌شدگان کاربر
- `GET /api/referrals/mine?page=1&pageSize=20`
- نیازمند احراز هویت است.
- معرف بر اساس کاربر جاری تعیین می‌شود؛ کلاینت نمی‌تواند با تغییر شناسه، فهرست معرف دیگری را درخواست کند.
- هر ردیف شامل مشخصات دعوت‌شده، زمان ثبت‌نام و وضعیت اشتراک خریداری‌شده است: `hasPurchasedSubscription`, `subscriptionActive`, `subscriptionPlanType`, `subscriptionEndAt`.

### گزارش مدیریتی
- `GET /api/admin/referrals?page=1&pageSize=50`
- محدود به نقش `SystemAdmin` است.
- گزارش مشخص می‌کند هر کاربر با کد دعوت چه کسی ثبت‌نام کرده، تعداد معرفی‌های هر معرف و وضعیت خرید/فعال بودن اشتراک کاربر دعوت‌شده چیست.
- فیلدهای مهم: `referrerName`, `referrerPhoneNumber`, `referrerInviteCode`, `referredName`, `referredPhoneNumber`, `totalReferredByReferrer`, `rewardGranted`, `rewardDays` و وضعیت اشتراک.

### تنظیمات طرح معرفی
- `GET /api/admin/referrals/settings`
- `PUT /api/admin/referrals/settings`
- هر دو فقط برای `SystemAdmin` هستند. بدنهٔ به‌روزرسانی: `isEnabled`, `rewardDays`, `maxRewardDaysPerUser`؛ مقادیر روز پاداش منفی پذیرفته نمی‌شوند.

## تعریف وضعیت اشتراک در گزارش
- `hasPurchasedSubscription` زمانی درست است که برای کاربر دعوت‌شده رکورد اشتراک خریداری‌شده وجود داشته باشد.
- `subscriptionActive` نشان می‌دهد همان اشتراک خریداری‌شده در زمان بررسی فعال است.
- نوع و زمان پایان اشتراک در صورت وجود رکورد برگردانده می‌شوند.
- ثبت‌نام از طریق دعوت به‌تنهایی به معنی خرید اشتراک نیست.

## دسترسی SystemAdmin
فیلتر `RequireActiveSubscription` برای نقش `SystemAdmin` بدون مطالبهٔ اشتراک فعال عبور می‌کند. این استثنا مربوط به همین فیلتر است و جایگزین احراز هویت/مجوز نقش در endpointهای مدیریتی نیست؛ endpointهای مدیریتی همچنان باید با `[Authorize(Roles = nameof(UserRole.SystemAdmin))]` محافظت شوند.

## اعتبارسنجی و صفحه‌بندی
- `page` باید حداقل ۱ باشد.
- `pageSize` باید بین ۱ تا ۱۰۰ باشد.
- پیش‌فرض فهرست کاربر `20` و پیش‌فرض گزارش مدیریت `50` است.

## وضعیت بررسی
این مستند بر اساس قراردادها و پیاده‌سازی موجود در کد نوشته شده است. اجرای `dotnet build` و تست‌ها در محیط تهیهٔ این مستند انجام نشده است.
