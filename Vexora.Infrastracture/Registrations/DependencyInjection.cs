using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vexora.Application.Conversations.Interfaces;
using Vexora.Application.Repositories.Customers;
using Vexora.Application.Repositories.Payments;
using Vexora.Application.Services.Interfaces.CurrentUser;
using Vexora.Application.UnitOfWorks;
using Vexora.Infrastracture.Caching.Conversations;
using Vexora.Infrastracture.Percistance.SqlServer.EfCore.Context;
using Vexora.Infrastracture.Percistance.SqlServer.EfCore.Repositories.Customers;
using Vexora.Infrastracture.Percistance.SqlServer.EfCore.Repositories.Payments;
using Vexora.Infrastracture.Percistance.SqlServer.EfCore.UnitOfWorks;

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

        services.AddScoped<ICurrentUser, DevelopmentCurrentUser>();

        services.AddScoped<ICustomerRepository, CustomerRepository>();

        services.AddScoped<IConversationStateStore,RedisConversationStateStore>();

        services.AddScoped<IPaymentRepository,PaymentRepository>();
        services.AddScoped<IUnitOfWork,UnitOfWork>();
    }
}
