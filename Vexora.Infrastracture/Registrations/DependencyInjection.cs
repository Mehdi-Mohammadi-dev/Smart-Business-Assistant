using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vexora.Application.Conversations.Interfaces;
using Vexora.Application.Repositories.Authentications;
using Vexora.Application.Repositories.Checks;
using Vexora.Application.Repositories.Customers;
using Vexora.Application.Repositories.Payments;
using Vexora.Application.Repositories.Users;
using Vexora.Application.Services.Interfaces.Authentications;
using Vexora.Application.Services.Interfaces.CurrentUser;
using Vexora.Application.UnitOfWorks;
using Vexora.Infrastracture.Caching.Conversations;
using Vexora.Infrastracture.Percistance.SqlServer.EfCore.Context;
using Vexora.Infrastracture.Percistance.SqlServer.EfCore.Repositories.Authentications;
using Vexora.Infrastracture.Percistance.SqlServer.EfCore.Repositories.Checks;
using Vexora.Infrastracture.Percistance.SqlServer.EfCore.Repositories.Customers;
using Vexora.Infrastracture.Percistance.SqlServer.EfCore.Repositories.Payments;
using Vexora.Infrastracture.Percistance.SqlServer.EfCore.Repositories.Users;
using Vexora.Infrastracture.Percistance.SqlServer.EfCore.UnitOfWorks;
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

        services.Configure<TsmsOptions>(configuration.GetSection("Tsms"));
        services.AddScoped<ISmsSender, TsmsSmsSender>();

        services.AddScoped<ICurrentUser, DevelopmentCurrentUser>();

        services.AddScoped<ICheckRepository, CheckRepository>();

        services.AddScoped<ICustomerRepository, CustomerRepository>();

        services.AddScoped<IConversationStateStore,RedisConversationStateStore>();

        services.AddScoped<IPaymentRepository,PaymentRepository>();
        services.AddScoped<IUnitOfWork,UnitOfWork>();
        
        services.AddScoped<IUserRepository,UserRepository>();
        services.AddScoped<IOtpVerificationRepository, OtpVerificationRepository>();  

    }
}
