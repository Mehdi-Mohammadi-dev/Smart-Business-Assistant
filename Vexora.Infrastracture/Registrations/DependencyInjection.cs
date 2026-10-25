using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vexora.Application.Caching.Authentications;
using Vexora.Application.Caching.Conversations.Interfaces;
using Vexora.Application.Options;
using Vexora.Application.Platform.Statements;
using Vexora.Application.Repositories.Authentications;
using Vexora.Application.Repositories.Checks;
using Vexora.Application.Repositories.Customers;
using Vexora.Application.Repositories.Payments;
using Vexora.Application.Repositories.RefreshTokens;
using Vexora.Application.Repositories.Subscriptions;
using Vexora.Application.Repositories.Users;
using Vexora.Application.Services.Interfaces.Authentications;
using Vexora.Application.UnitOfWorks;
using Vexora.Infrastracture.Caching.Authentications;
using Vexora.Infrastracture.Caching.Conversations;
using Vexora.Infrastracture.Pdf;
using Vexora.Infrastracture.Percistance.SqlServer.EfCore.Context;
using Vexora.Infrastracture.Percistance.SqlServer.EfCore.Repositories.Authentications;
using Vexora.Infrastracture.Percistance.SqlServer.EfCore.Repositories.Checks;
using Vexora.Infrastracture.Percistance.SqlServer.EfCore.Repositories.Customers;
using Vexora.Infrastracture.Percistance.SqlServer.EfCore.Repositories.Payments;
using Vexora.Infrastracture.Percistance.SqlServer.EfCore.Repositories.RefreshTokens;
using Vexora.Infrastracture.Percistance.SqlServer.EfCore.Repositories.Subscriptions;
using Vexora.Infrastracture.Percistance.SqlServer.EfCore.Repositories.Users;
using Vexora.Infrastracture.Percistance.SqlServer.EfCore.UnitOfWorks;
using Vexora.Infrastracture.Security;
using Vexora.Infrastracture.Sms;
using Vexora.Infrastracture.Sms.Tsms;

namespace Vexora.Infrastracture.Registrations;

public static class DependencyInjection
{
    public static void AddInfrastracture(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration =
                configuration.GetConnectionString("Redis");
        });

        services.AddScoped<IPendingRegistrationStore,RedisPendingRegistrationStore>();
        services.AddScoped<ILoginAttemptStore, RedisLoginAttemptStore>();

        services.Configure<TsmsOptions>(configuration.GetSection("Tsms"));

        if (string.Equals(configuration["Sms:Provider"], "Fake", StringComparison.OrdinalIgnoreCase))
            services.AddScoped<ISmsSender, FakeSmsSender>();
        else
            services.AddScoped<ISmsSender, TsmsSmsSender>();

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddSingleton<ITokenService, JwtTokenService>();

        services.AddScoped<ICheckRepository, CheckRepository>();

        services.AddScoped<ICustomerRepository, CustomerRepository>();

        services.AddScoped<IConversationStateStore,RedisConversationStateStore>();

        services.AddScoped<IPaymentRepository,PaymentRepository>();
        services.AddScoped<IUnitOfWork,UnitOfWork>();

        services.AddScoped<IUserRepository,UserRepository>();
        services.AddScoped<IOtpVerificationRepository, OtpVerificationRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        services.AddScoped<IAccessTokenRevocationStore, RedisAccessTokenRevocationStore>();

        services.AddScoped<IStatementPdfGenerator, QuestPdfStatementGenerator>();
    }
}
