# راهنمای اتصال فرانت‌اند

همه پاسخ‌ها: `{ success, message, data, errors[] }`. احراز هویت: `Authorization: Bearer <token>`.

## گردش حساب مشتری
- نمایش: `GET /api/customers/{id}/statement?from=2026-01-01&to=2026-03-31`
- PDF: `GET /api/customers/{id}/statement/pdf?from=...&to=...`
- `from` و `to` اختیاری (میلادی، `yyyy-MM-dd`). اگر نباشند، کل بازه. `from > to` ← 400.
- PDF را با `responseType: blob` بگیرید. بیش از ۵۰۰۰ ردیف ← خطا؛ بازه را کوچک‌تر کنید.
- نام کسب‌وکار در سربرگ PDF می‌آید. فقط دریافتی‌ها ثبت می‌شود (بدهی نمایش داده نمی‌شود).

## پلن‌ها
- عمومی: `GET /api/plans` (بدون توکن) ← `id, name, days, priceToman, discountPercent, finalPriceToman, pricePerDayToman, isFeatured`
- ادمین (نقش SystemAdmin): `GET/POST /api/admin/plans`, `PUT/DELETE /api/admin/plans/{id}`
- بدنه: `{ name, days, priceToman, discountPercent, isActive, isFeatured, sortOrder }`
- قیمت همیشه از سرور؛ هرگز مبلغ را از فرانت نفرستید. مبالغ به تومان.
- خطاها: 400 مقدار نامعتبر، 401/403 دسترسی، 404 پلن نیست.

## معرفی دوستان
جدول‌ها آماده‌اند؛ endpoint هنوز نیست.

## ثبت نام
`BusinessName` در ثبت‌نام اختیاری است.
