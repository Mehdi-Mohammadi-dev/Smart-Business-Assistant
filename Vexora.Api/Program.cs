using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using System.Threading.RateLimiting;
using Vexora.Api.Commons.Filters;
using Vexora.Api.Commons.Middelwares;
using Vexora.Api.Commons.Results;
using Vexora.Api.Services.CurrentUser;
using Vexora.Api.Validators.Chats;
using Vexora.Application.Caching.Authentications;
using Vexora.Application.DependencyInjections;
using Vexora.Application.Options;
using Vexora.Application.Services.Interfaces.CurrentUser;
using Vexora.Infrastracture.Registrations;

var builder = WebApplication.CreateBuilder(args);

// ---------- Services ----------
builder.Services
    .AddControllers(options =>
    {
        options.Filters.Add<ValidationFilter>();
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.SuppressModelStateInvalidFilter = true;
    });

builder.Services.AddSwaggerGen(options =>
{
    // 1) Tell Swagger that this API uses a JWT "Bearer" token
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "فقط accessToken را وارد کنید (کلمه Bearer را ننویسید)."
    });

    // 2) Send that token with the requests
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});
builder.Services.AddOpenApi();

builder.Services.AddInfrastracture(builder.Configuration);
builder.Services.AddApplication();

builder.Services.AddValidatorsFromAssemblyContaining<ChatRequestValidator>();

// ---------- Authentication (JWT) ----------
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
        // Keep the claim names exactly as we wrote them in the token ("sub", "role", ...)
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

                Console.WriteLine(
                    $"JWT validated. jti: {tokenId ?? "NULL"}");

                try
                {
                    var revocationStore = context.HttpContext
                        .RequestServices
                        .GetRequiredService<IAccessTokenRevocationStore>();

                    var isRevoked = await revocationStore.IsRevokedAsync(
                        tokenId!,
                        context.HttpContext.RequestAborted);

                    Console.WriteLine($"Token revoked: {isRevoked}");

                    if (isRevoked)
                        context.Fail("Access token has been revoked.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Token validation error: {ex}");

                    context.Fail(
                        "Access token validation is temporarily unavailable.");
                }
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, JwtCurrentUser>();

// ---------- Rate limiting (protects the SMS credit from abuse) ----------
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("otp", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));

    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.ContentType = "application/json; charset=utf-8";

        await context.HttpContext.Response.WriteAsJsonAsync(
            ApiResult<object>.Failure(
                "تعداد درخواست‌ها زیاد است. کمی بعد دوباره تلاش کنید.",
                new ApiError("TOO_MANY_REQUESTS", "تعداد درخواست‌ها زیاد است.")),
            cancellationToken);
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "Frontend",
        policy =>
        {
            policy
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

// ---------- Pipeline (the ORDER matters) ----------
var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("Frontend");

app.UseRateLimiter();

app.UseAuthentication();   // who are you?   (reads the JWT)
app.UseAuthorization();    // are you allowed? ([Authorize])

app.MapControllers();

app.Run();
