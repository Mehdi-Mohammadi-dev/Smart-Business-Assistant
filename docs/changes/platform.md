# Platform changes

Implemented in this change set:
- Added subscription expiry exception mapping and an active-subscription action filter with per-request caching.
- Added digit and phone normalizers for Persian and Arabic digits; wired them into authentication request mapping and OTP verification.
- Added Redis-backed login lockout, refresh-token replay revocation, password-reset OTP endpoints, and refresh-token bulk revocation.
- Added admin user listing and subscription extend/cancel endpoints, plus environment-driven first-admin initialization.
- Added rate limit policies, CORS allowlist configuration, Swagger gating, forwarded headers configuration, security headers, request size limit, health endpoints, structured JSON console and rolling-file logging.
- Added statement DTO contracts, QuestPDF statement generator, and PDF endpoint contract. `IStatementService` implementation is intentionally not included because it belongs to the ledger stream.
- QuestPDF Community terms were reviewed at https://www.questpdf.com/license/community.html and https://www.questpdf.com/license/guide.html. Community use requires eligibility (typically annual gross revenue under USD 1 million; public-sector and publicly traded entities are excluded). The project owner must confirm the legal entity qualifies before production use.
- Added security documentation, error-code table, and normalizer tests.

Verification limitations:
- The .NET SDK was not available in the execution environment, so `dotnet build` and `dotnet test` could not be run.
- The source archive did not include Vazirmatn font files. The PDF generator registers those files when supplied, but Persian shaping and the required Chrome/Acrobat visual check remain unverified.
- No Docker, compose, environment example, deployment, or CI/release files were changed.

## تغییرات تحویل نهایی
- صورتحساب مشتری: `GET /api/customers/{id}/statement` (JSON، از قبل در CustomersController) و `GET /api/customers/{id}/statement/pdf` (PDF). مسیر قدیمی `statement.pdf` حذف شد.
- حذف `Console.WriteLine` که کد OTP را در لاگ چاپ می‌کرد (`AuthService`).
- پیامک فقط برای ثبت‌نام/بازیابی رمز؛ متن شامل نام «نوتا».
- بدون build و تست در محیط توسعه؛ اجرا: `dotnet test Vexora.Application.Test`.
