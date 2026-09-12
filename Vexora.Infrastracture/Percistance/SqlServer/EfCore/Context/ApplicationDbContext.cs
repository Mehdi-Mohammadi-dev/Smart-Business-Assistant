using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Vexora.Domain.Entities.Checks;
using Vexora.Domain.Entities.Customers;
using Vexora.Domain.Entities.Payments;
using Vexora.Domain.Entities.Sales;
using Vexora.Domain.Entities.Sales.Items;
using Vexora.Domain.Entities.Users;

namespace Vexora.Infrastracture.Percistance.SqlServer.EfCore.Context;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Customer> Customers{ get; set; }
    public DbSet<Sale> Sales{ get; set; }
    public DbSet<SaleItem> SaleItems{ get; set; }
    public DbSet<Check> Checks{ get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
