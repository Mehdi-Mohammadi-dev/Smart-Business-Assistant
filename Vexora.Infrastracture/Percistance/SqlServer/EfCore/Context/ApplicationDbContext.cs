using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Vexora.Domain.Entities.Authentications;
using Vexora.Domain.Entities.Checks;
using Vexora.Domain.Entities.Customers;
using Vexora.Domain.Entities.Payments;
using Vexora.Domain.Entities.RefreshTokens;
using Vexora.Domain.Entities.Sales;
using Vexora.Domain.Entities.Sales.Items;
using Vexora.Domain.Entities.Sunscriptions;
using Vexora.Domain.Entities.Users;
using Vexora.Domain.sharedKernel.BaseEntities;

namespace Vexora.Infrastracture.Percistance.SqlServer.EfCore.Context;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Customer> Customers{ get; set; }
    public DbSet<Sale> Sales{ get; set; }
    public DbSet<SaleItem> SaleItems{ get; set; }
    public DbSet<Check> Checks{ get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<OtpVerification> OtpVerifications { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Subscription> Subscriptions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                continue;

            var parameter = System.Linq.Expressions.Expression.Parameter(
                entityType.ClrType, "e");

            var property = System.Linq.Expressions.Expression.Property(
                parameter, nameof(BaseEntity.IsDelete));

            var condition = System.Linq.Expressions.Expression.Equal(
                property,
                System.Linq.Expressions.Expression.Constant(false));

            var filter = System.Linq.Expressions.Expression.Lambda(
                condition, parameter);

            modelBuilder.Entity(entityType.ClrType)
                .HasQueryFilter(filter);
        }
    }

    public override int SaveChanges()
    {
        ApplyAuditFieldsAndSoftDelete();
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyAuditFieldsAndSoftDelete();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        ApplyAuditFieldsAndSoftDelete();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        ApplyAuditFieldsAndSoftDelete();

        return base.SaveChangesAsync(
            acceptAllChangesOnSuccess,
            cancellationToken);
    }

    private void ApplyAuditFieldsAndSoftDelete()
    {
        var now = DateTimeOffset.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property(nameof(BaseEntity.CreateAt)).CurrentValue = now;
                entry.Property(nameof(BaseEntity.UpdateAt)).CurrentValue = now;
                entry.Property(nameof(BaseEntity.IsDelete)).CurrentValue = false;

                continue;
            }

            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;

                entry.Property(nameof(BaseEntity.IsDelete)).CurrentValue = true;
                entry.Property(nameof(BaseEntity.IsDelete)).IsModified = true;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Property(nameof(BaseEntity.UpdateAt)).CurrentValue = now;
                entry.Property(nameof(BaseEntity.UpdateAt)).IsModified = true;

                entry.Property(nameof(BaseEntity.CreateAt)).IsModified = false;
            }
        }
    }
}
