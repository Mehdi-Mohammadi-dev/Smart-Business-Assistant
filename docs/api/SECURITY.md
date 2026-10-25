# Platform security

- Authentication uses signed JWT access tokens and refresh tokens stored as hashes.
- Reusing a revoked refresh token revokes all refresh tokens for that user and records a security warning without logging the token.
- Login attempts are counted in Redis per normalized phone number; five failed attempts lock that number for ten minutes.
- OTP, registration, login, refresh, and chat endpoints have named rate-limit policies.
- Write endpoints can use `RequireActiveSubscription`; missing, cancelled, or expired subscriptions return HTTP 402. Read endpoints and statement PDF remain available.
- CORS origins are configured using `Cors:AllowedOrigins`. Keep the list explicit in production.
- Swagger is available only in Development or when `Swagger:Enabled=true`.
- Responses include `nosniff`, `no-referrer`, `DENY`, and production HSTS headers. Request bodies above 64 KiB are rejected except under `/api/uploads`.
- `/health/live` exposes liveness only. `/health/ready` exposes a generic readiness status for SQL Server and Redis.
- Logs are JSON to console and daily rolling files. Request logs contain request ID, user ID when authenticated, path, status, and duration; do not log message text, money, credentials, OTPs, or tokens.
- `Admin__Phone` and `Admin__Password` should be supplied only through environment variables. Initial admin creation is skipped unless both are present and no system admin exists.
