using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Vexora.Infrastracture.Percistance.SqlServer.EfCore.Context;

namespace Vexora.Infrastracture.Percistance.Registrations;

public static class DependencyInjection
{
    public static void AddInfrastracture(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
    }
}
