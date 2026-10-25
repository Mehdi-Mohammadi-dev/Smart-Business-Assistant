using Vexora.Application.Ledger.Checks;
using Vexora.Application.Ledger.Models;
using Vexora.Domain.Entities.Checks;
using Vexora.Domain.Enums.BankRegistrationStatuses;
using Vexora.Domain.Enums.CheckSettlementStatuses;
using Vexora.Domain.Enums.TransactionDirections;

namespace Vexora.Application.Ledger.Queries;

public static class CheckQueries
{
    public static IQueryable<Check> Filter(
        IQueryable<Check> source,
        long userId,
        CheckListQuery query,
        DateTime today)
    {
        var todayDate = today.Date;
        var result = source.Where(x => x.UserId == userId);

        if (query.CustomerId is { } customerId)
            result = result.Where(x => x.CustomerId == customerId);

        if (query.Settlement is { } settlement)
            result = result.Where(x => x.SettlementStatus == settlement);

        if (query.Registration is { } registration)
            result = result.Where(x => x.BankRegistrationStatus == registration);

        if (query.DueFrom is { } dueFrom)
        {
            var start = dueFrom.Date;
            result = result.Where(x => x.DueDate >= start);
        }

        if (query.DueTo is { } dueTo)
        {
            var endExclusive = dueTo.Date.AddDays(1);
            result = result.Where(x => x.DueDate < endExclusive);
        }

        if (query.Overdue)
        {
            result = result.Where(x =>
                x.SettlementStatus == CheckSettlementStatus.Pending &&
                x.DueDate < todayDate);
        }

        if (query.DueWithinDays is { } days)
        {
            var windowEndExclusive = todayDate.AddDays(days + 1);

            result = result.Where(x =>
                x.SettlementStatus == CheckSettlementStatus.Pending &&
                x.DueDate >= todayDate &&
                x.DueDate < windowEndExclusive);
        }

        if (!string.IsNullOrEmpty(query.Search))
        {
            var term = query.Search;

            result = result.Where(x =>
                (x.CheckNumber != null && x.CheckNumber.Contains(term)) ||
                (x.Customer.FirstName + " " + x.Customer.LastName)
                    .Replace("ي", "ی")
                    .Replace("ك", "ک")
                    .Replace("\u200c", " ")
                    .Contains(term));
        }

        return result;
    }

    public static IOrderedQueryable<Check> Sort(
        IQueryable<Check> source,
        CheckSort sort)
        => sort == CheckSort.CreatedAt
            ? source.OrderByDescending(x => x.CreateAt).ThenByDescending(x => x.Id)
            : source.OrderBy(x => x.DueDate).ThenBy(x => x.Id);

    public static IQueryable<CheckRow> ToRows(IQueryable<Check> source)
        => source.Select(x => new CheckRow
        {
            Id = x.Id,
            CustomerId = x.CustomerId,
            CustomerFirstName = x.Customer.FirstName,
            CustomerLastName = x.Customer.LastName,
            Amount = x.Amount,
            DueDate = x.DueDate,
            CheckNumber = x.CheckNumber,
            Direction = x.Direction,
            SettlementStatus = x.SettlementStatus,
            BankRegistration = x.BankRegistrationStatus,
            CreatedAt = x.CreateAt,
            PaidAt = x.PaidAt,
            BouncedAt = x.BouncedAt
        });

    public static IQueryable<CheckAggregate> ByCustomer(
        IQueryable<Check> source,
        long userId,
        long? customerId,
        DateTime today)
    {
        var todayDate = today.Date;

        var received = source.Where(x =>
            x.UserId == userId &&
            x.Direction == TransactionDirection.Received);

        if (customerId is { } id)
            received = received.Where(x => x.CustomerId == id);

        return received
            .GroupBy(x => x.CustomerId)
            .Select(g => new CheckAggregate
            {
                CustomerId = g.Key,
                PendingTotal = g
                    .Where(x => x.SettlementStatus == CheckSettlementStatus.Pending)
                    .Sum(x => x.Amount),
                PendingCount = g
                    .Count(x => x.SettlementStatus == CheckSettlementStatus.Pending),
                OverdueCount = g
                    .Count(x => x.SettlementStatus == CheckSettlementStatus.Pending &&
                                x.DueDate < todayDate),
                PaidTotal = g
                    .Where(x => x.SettlementStatus == CheckSettlementStatus.Paid)
                    .Sum(x => x.Amount),
                LastAt = g.Max(x => x.CreateAt)
            });
    }

    public static IQueryable<CheckPendingAggregate> PendingSummary(
        IQueryable<Check> source,
        long userId,
        DateTime today,
        int dueSoonDays)
    {
        var todayDate = today.Date;
        var soonEndExclusive = todayDate.AddDays(dueSoonDays + 1);

        return source
            .Where(x =>
                x.UserId == userId &&
                x.Direction == TransactionDirection.Received &&
                x.SettlementStatus == CheckSettlementStatus.Pending)
            .GroupBy(x => x.UserId)
            .Select(g => new CheckPendingAggregate
            {
                PendingCount = g.Count(),
                PendingTotal = g.Sum(x => x.Amount),
                OverdueCount = g.Count(x => x.DueDate < todayDate),
                OverdueTotal = g.Where(x => x.DueDate < todayDate).Sum(x => x.Amount),
                DueSoonCount = g.Count(x =>
                    x.DueDate >= todayDate && x.DueDate < soonEndExclusive),
                DueSoonTotal = g
                    .Where(x => x.DueDate >= todayDate && x.DueDate < soonEndExclusive)
                    .Sum(x => x.Amount),
                NotRegisteredCount = g.Count(x =>
                    x.BankRegistrationStatus == BankRegistrationStatus.NotRegistered)
            });
    }

    public static IQueryable<StatementCheckRow> ForStatement(
        IQueryable<Check> source,
        long userId,
        long customerId,
        DateTime? from,
        DateTime? to)
    {
        var result = source.Where(x =>
            x.UserId == userId &&
            x.CustomerId == customerId &&
            x.Direction == TransactionDirection.Received);

        if (from is { } fromDay)
        {
            var start = fromDay.Date;
            result = result.Where(x => x.DueDate >= start);
        }

        if (to is { } toDay)
        {
            var endExclusive = toDay.Date.AddDays(1);
            result = result.Where(x => x.DueDate < endExclusive);
        }

        return result.Select(x => new StatementCheckRow
        {
            Id = x.Id,
            CreatedAt = x.CreateAt,
            Amount = x.Amount,
            DueDate = x.DueDate,
            CheckNumber = x.CheckNumber,
            SettlementStatus = x.SettlementStatus,
            BankRegistration = x.BankRegistrationStatus
        });
    }
}
