using Vexora.Application.Ledger.Models;
using Vexora.Application.Ledger.Payments;
using Vexora.Domain.Entities.Payments;
using Vexora.Domain.Enums.TransactionDirections;

namespace Vexora.Application.Ledger.Queries;

public static class PaymentQueries
{
    public static IQueryable<Payment> Filter(
        IQueryable<Payment> source,
        long userId,
        PaymentListQuery query)
    {
        var result = source.Where(x => x.UserId == userId);

        if (query.CustomerId is { } customerId)
            result = result.Where(x => x.CustomerId == customerId);

        var bounds = DateRangeBounds.FromIranDays(query.From, query.To);

        if (bounds.FromUtc is { } fromUtc)
            result = result.Where(x => x.PaymentDate >= fromUtc);

        if (bounds.ToUtcExclusive is { } toUtc)
            result = result.Where(x => x.PaymentDate < toUtc);

        return result;
    }

    public static IQueryable<PaymentRow> ToRows(IQueryable<Payment> source)
        => source.Select(x => new PaymentRow
        {
            Id = x.Id,
            CustomerId = x.CustomerId,
            CustomerFirstName = x.Customer.FirstName,
            CustomerLastName = x.Customer.LastName,
            Amount = x.Amount,
            PaymentDate = x.PaymentDate,
            Direction = x.Direction
        });

    public static IQueryable<PaymentAggregate> ByCustomer(
        IQueryable<Payment> source,
        long userId,
        long? customerId)
    {
        var received = source.Where(x =>
            x.UserId == userId &&
            x.Direction == TransactionDirection.Received);

        if (customerId is { } id)
            received = received.Where(x => x.CustomerId == id);

        return received
            .GroupBy(x => x.CustomerId)
            .Select(g => new PaymentAggregate
            {
                CustomerId = g.Key,
                Total = g.Sum(x => x.Amount),
                LastAt = g.Max(x => x.CreateAt)
            });
    }

    public static IQueryable<PaymentPeriodAggregate> Period(
        IQueryable<Payment> source,
        long userId,
        DateTimeOffset monthStartUtc,
        DateTimeOffset todayStartUtc,
        DateTimeOffset tomorrowStartUtc)
    {
        return source
            .Where(x =>
                x.UserId == userId &&
                x.Direction == TransactionDirection.Received &&
                x.PaymentDate >= monthStartUtc &&
                x.PaymentDate < tomorrowStartUtc)
            .GroupBy(x => x.UserId)
            .Select(g => new PaymentPeriodAggregate
            {
                MonthCount = g.Count(),
                MonthTotal = g.Sum(x => x.Amount),
                TodayTotal = g.Where(x => x.PaymentDate >= todayStartUtc).Sum(x => x.Amount)
            });
    }

    public static IQueryable<StatementPaymentRow> ForStatement(
        IQueryable<Payment> source,
        long userId,
        long customerId,
        DateRangeBounds bounds)
    {
        var result = source.Where(x =>
            x.UserId == userId &&
            x.CustomerId == customerId &&
            x.Direction == TransactionDirection.Received);

        if (bounds.FromUtc is { } fromUtc)
            result = result.Where(x => x.PaymentDate >= fromUtc);

        if (bounds.ToUtcExclusive is { } toUtc)
            result = result.Where(x => x.PaymentDate < toUtc);

        return result.Select(x => new StatementPaymentRow
        {
            Id = x.Id,
            PaymentDate = x.PaymentDate,
            Amount = x.Amount
        });
    }
}
