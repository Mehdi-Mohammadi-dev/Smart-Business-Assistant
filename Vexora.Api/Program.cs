using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using Serilog.Context;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Vexora.Api.Commons.Filters;
using Vexora.Api.Commons.Middelwares;
using Vexora.Api.Commons.Results;
using Vexora.Api.Platform;
using Vexora.Api.Services.CurrentUser;
using Vexora.Api.Validators.Chats;
using Vexora.Application.Caching.Authentications;
using Vexora.Application.DependencyInjections;
using Vexora.Application.Options;
using Vexora.Application.Services.Interfaces.CurrentUser;
using Vexora.Infrastracture.Registrations;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((context, services, configuration) => configuration
    .MinimumLevel.Is(Enum.TryParse<Serilog.Events.LogEventLevel>(context.Configuration["Logging:LogLevel:Default"], true, out var level) ? level : Serilog.Events.LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console(new Serilog.Formatting.Compact.CompactJsonFormatter())
    .WriteTo.File(new Serilog.Formatting.Compact.CompactJsonFormatter(), "logs/platform-.json", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 14, shared: true));

builder.Services
    .AddControllers(options =>
    {
        options.Filters.Add<ValidationFilter>();
    })
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
    .ConfigureApiBehaviorOptions(options =>
    {
        options.SuppressModelStateInvalidFilter = true;
    });

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "فقط accessToken را وارد کنید (کلمه Bearer را ننویسید)."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});
builder.Services.AddOpenApi();

builder.Services.AddInfrastracture(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddPlatformServices();

builder.Services.AddValidatorsFromAssemblyContaining<ChatRequestValidator>();

if (!builder.Environment.IsDevelopment() && string.Equals(builder.Configuration["Sms:Provider"], "Fake", StringComparison.OrdinalIgnoreCase))
    throw new InvalidOperationException("Fake SMS sender is only allowed in Development.");

var jwt = builder.Configuration
              .GetSection(JwtOptions.SectionName)
              .Get<JwtOptions>() ?? new JwtOptions();

if (string.IsNullOrWhiteSpace(jwt.Secret) || jwt.Secret.Length < 32)
{
    throw new InvalidOperationException(
        "Jwt:Secret is missing or shorter than 32 characters. " +
        "Set it with: dotnet user-secrets set \"Jwt:Secret\" \"<a long random text>\"");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = "sub",
            RoleClaimType = "role"
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var tokenId = context.Principal?
                    .FindFirst("jti")?.Value;

                try
                {
                    var revocationStore = context.HttpContext
                        .RequestServices
                        .GetRequiredService<IAccessTokenRevocationStore>();

                    var isRevoked = await revocationStore.IsRevokedAsync(
                        tokenId!,
                        context.HttpContext.RequestAborted);

                    if (isRevoked)
                        context.Fail("Access token has been revoked.");
                }
                catch
                {
                    context.Fail(
                        "Access token validation is temporarily unavailable.");
                }
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, JwtCurrentUser>();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    void AddIpPolicy(string name, int limit) =>
        options.AddPolicy(name, context => RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions { PermitLimit = limit, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
    AddIpPolicy("otp", 5);
    AddIpPolicy("login", 10);
    AddIpPolicy("register", 10);
    AddIpPolicy("refresh", 30);
    options.AddPolicy("chat", context =>
    {
        var key = context.User.FindFirst("sub")?.Value ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
        return RateLimitPartition.GetFixedWindowLimiter(key,
            _ => new FixedWindowRateLimiterOptions { PermitLimit = 30, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 });
    });
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.ContentType = "application/json; charset=utf-8";
        await context.HttpContext.Response.WriteAsJsonAsync(
            ApiResult<object>.Failure("تعداد درخواست‌ها زیاد است. کمی بعد دوباره تلاش کنید.",
                new ApiError("TOO_MANY_REQUESTS", "تعداد درخواست‌ها زیاد است.")), cancellationToken);
    };
});

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options => options.AddPolicy("Frontend", policy =>
{
    if (allowedOrigins.Length > 0)
        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
}));

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownProxies.Clear();
    options.KnownIPNetworks.Clear();
    foreach (var proxy in builder.Configuration.GetSection("ForwardedHeaders:KnownProxies").Get<string[]>() ?? [])
        if (System.Net.IPAddress.TryParse(proxy, out var address))
            options.KnownProxies.Add(address);
});

builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "live" })
    .AddCheck<Vexora.Api.Platform.SqlServerHealthCheck>("sqlserver", tags: new[] { "ready" })
    .AddCheck<Vexora.Api.Platform.RedisHealthCheck>("redis", tags: new[] { "ready" });

var app = builder.Build();

app.UseForwardedHeaders();
app.Use(async (context, next) =>
{
    context.Request.Headers.TryAdd("X-Request-Id", context.TraceIdentifier);
    context.Response.Headers["X-Request-Id"] = context.TraceIdentifier;
    using var requestIdScope = LogContext.PushProperty("requestId", context.TraceIdentifier);
    if (!context.Request.Path.StartsWithSegments("/api/uploads"))
    {
        var sizeFeature = context.Features.Get<Microsoft.AspNetCore.Http.Features.IHttpMaxRequestBodySizeFeature>();
        if (sizeFeature is { IsReadOnly: false })
            sizeFeature.MaxRequestBodySize = 65536;
    }
    if (context.Request.ContentLength > 65536 && !context.Request.Path.StartsWithSegments("/api/uploads"))
    {
        context.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;
        await context.Response.WriteAsJsonAsync(ApiResult<object>.Failure("حجم درخواست بیش از حد مجاز است.",
            new ApiError("REQUEST_TOO_LARGE", "حداکثر اندازه درخواست ۶۴ کیلوبایت است.")));
        return;
    }
    context.Response.OnStarting(() =>
    {
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        context.Response.Headers["Referrer-Policy"] = "no-referrer";
        context.Response.Headers["X-Frame-Options"] = "DENY";
        if (!app.Environment.IsDevelopment())
            context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
        return Task.CompletedTask;
    });
    await next();
});
if (app.Environment.IsDevelopment() || builder.Configuration.GetValue<bool>("Swagger:Enabled"))
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("Frontend");

app.UseAuthentication();
app.Use(async (context, next) =>
{
    using var userIdScope = LogContext.PushProperty("userId", context.User.FindFirst("sub")?.Value);
    var started = System.Diagnostics.Stopwatch.GetTimestamp();
    try { await next(); }
    finally
    {
        var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("RequestLogging");
        logger.LogInformation("HTTP {Method} {Path} responded {StatusCode} in {ElapsedMilliseconds} ms",
            context.Request.Method, context.Request.Path, context.Response.StatusCode,
            System.Diagnostics.Stopwatch.GetElapsedTime(started).TotalMilliseconds);
    }
});
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
app.UseRateLimiter();
app.UseAuthorization();

app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live"),
    ResponseWriter = async (context, _) =>
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync("{\"status\":\"ok\"}");
    }
}).AllowAnonymous();
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { status = report.Status == HealthStatus.Healthy ? "ok" : "unavailable" });
    }
}).AllowAnonymous();

app.MapControllers();

var adminPhone = builder.Configuration["Admin:Phone"];
var adminPassword = builder.Configuration["Admin:Password"];
using (var scope = app.Services.CreateScope())
{
    var users = scope.ServiceProvider.GetRequiredService<Vexora.Application.Repositories.Users.IUserRepository>();
    if (!await users.HasSystemAdminAsync())
    {
        if (string.IsNullOrWhiteSpace(adminPhone) || string.IsNullOrWhiteSpace(adminPassword))
            throw new InvalidOperationException("Admin:Phone and Admin:Password environment variables are required until a SystemAdmin exists.");
        var hasher = scope.ServiceProvider.GetRequiredService<Vexora.Application.Services.Interfaces.Authentications.IPasswordHasher>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<Vexora.Application.UnitOfWorks.IUnitOfWork>();
        await users.InsertAsync(Vexora.Domain.Entities.Users.User.CreateSystemAdmin(
            Vexora.Application.Common.PhoneNumberNormalizer.Normalize(adminPhone), hasher.Hash(adminPassword)));
        await unitOfWork.SaveAsync();
    }
}

app.Run();
