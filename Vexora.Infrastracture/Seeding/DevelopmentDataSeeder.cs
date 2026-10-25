using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Vexora.Application.Common;
using Vexora.Application.Services.Interfaces.Authentications;
using Vexora.Domain.Entities.Checks;
using Vexora.Domain.Entities.Customers;
using Vexora.Domain.Entities.Payments;
using Vexora.Domain.Entities.Sunscriptions;
using Vexora.Domain.Entities.Users;
using Vexora.Domain.Enums.BankRegistrationStatuses;
using Vexora.Domain.Enums.TransactionDirections;
using Vexora.Infrastracture.Percistance.SqlServer.EfCore.Context;

namespace Vexora.Infrastracture.Seeding;

public sealed class DevelopmentDataSeeder(
    IServiceScopeFactory scopeFactory,
    IHostEnvironment environment,
    IConfiguration configuration,
    TimeProvider timeProvider,
    ILogger<DevelopmentDataSeeder> logger)
    : IHostedService
{
    public const string SeedPhoneNumber = "09120000000";

    public const string SeedPassword = "Seed@12345";

    private static readonly (string First, string Last)[] CustomerNames =
    [
        ("علی", "رضایی"),
        ("علی", "رضایی بنا"),
        ("مریم", "احمدی"),
        ("حسین", "کریمی"),
        ("زهرا", "محمدی"),
        ("رضا", "حسینی"),
        ("سارا", "موسوی"),
        ("محمد", "صادقی"),
        ("فاطمه", "جعفری"),
        ("امیر", "نوری"),
        ("نرگس", "قاسمی"),
        ("مهدی", "اکبری"),
        ("لیلا", "رحیمی"),
        ("بهرام", "شریفی"),
        ("پریسا", "یوسفی")
    ];

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var enabled = bool.TryParse(configuration["Seed:Enabled"], out var value) && value;

        if (!environment.IsDevelopment() || !enabled)
            return;

        try
        {
            await SeedAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Development seeding failed.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task SeedAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        if (await db.Users.AnyAsync(x => x.PhoneNumber == SeedPhoneNumber, cancellationToken))
            return;

        var user = User.Register("کاربر", "نمونه", SeedPhoneNumber, hasher.Hash(SeedPassword));
        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);

        db.Subscriptions.Add(Subscription.CreateTrial(user.Id));

        var customers = CustomerNames
            .Select(x => Customer.Create(x.First, x.Last, user.Id))
            .ToList();

        db.Customers.AddRange(customers);
        await db.SaveChangesAsync(cancellationToken);

        var nowUtc = timeProvider.GetUtcNow();
        var today = IranTime.Today(timeProvider);

        db.Payments.AddRange(BuildPayments(user.Id, customers, nowUtc));
        db.Checks.AddRange(BuildChecks(user.Id, customers, today, nowUtc));

        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Development data seeded for phone {PhoneNumber}.",
            SeedPhoneNumber);
    }

    private static List<Payment> BuildPayments(
        long userId,
        IReadOnlyList<Customer> customers,
        DateTimeOffset nowUtc)
    {
        var payments = new List<Payment>();

        for (var i = 0; i < 60; i++)
        {
            var customer = customers[i % customers.Count];
            var amount = (1 + i * 7 % 20) * 250_000m;
            var date = nowUtc.AddDays(-(i % 45)).AddMinutes(-(i % 9) * 30);

            payments.Add(Payment.Create(userId, customer.Id, amount, date));
        }

        return payments;
    }

    private static List<Check> BuildChecks(
        long userId,
        IReadOnlyList<Customer> customers,
        DateTime today,
        DateTimeOffset nowUtc)
    {
        var checks = new List<Check>();

        for (var i = 0; i < 40; i++)
        {
            var customer = customers[i * 3 % customers.Count];
            var amount = (2 + i % 9) * 1_000_000m;
            var number = (500000 + i * 17).ToString();

            var registration = i % 2 == 0
                ? BankRegistrationStatus.Registered
                : BankRegistrationStatus.NotRegistered;

            var dueDate = DueDateFor(i, today);

            var check = Check.Create(
                userId,
                customer.Id,
                amount,
                dueDate,
                number,
                TransactionDirection.Received,
                registration);

            if (i is >= 22 and <= 29)
                check.MarkAsPaid(nowUtc.AddDays(-2));
            else if (i is >= 30 and <= 34)
                check.MarkAsBounced(nowUtc.AddDays(-1));

            checks.Add(check);
        }

        return checks;
    }

    private static DateTime DueDateFor(int index, DateTime today)
        => index switch
        {
            < 5 => today.AddDays(-(index + 1) * 3),
            < 10 => today.AddDays(index - 5),
            < 15 => today.AddDays(6 + (index - 10) * 2),
            < 22 => today.AddDays(30 + (index - 15) * 15),
            < 30 => today.AddDays(-(index - 21) * 5),
            < 35 => today.AddDays(-(index - 29) * 4),
            _ => today.AddDays(10 + (index - 35) * 9)
        };
}
