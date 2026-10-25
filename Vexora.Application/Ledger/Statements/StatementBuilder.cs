using Vexora.Application.Common;
using Vexora.Application.Ledger.Models;
using Vexora.Domain.Enums.CheckSettlementStatuses;

namespace Vexora.Application.Ledger.Statements;

public static class StatementBuilder
{
    public const string PaymentType = "Payment";

    public const string CheckType = "Check";

    public static CustomerStatementDto Build(
        long customerId,
        string customerName,
        DateTime? from,
        DateTime? to,
        IReadOnlyCollection<StatementPaymentRow> payments,
        IReadOnlyCollection<StatementCheckRow> checks,
        DateTimeOffset generatedAt)
    {
        var rows = new List<StatementRowDto>(payments.Count + checks.Count);

        foreach (var payment in payments)
        {
            var date = IranTime.ToIranDateTime(payment.PaymentDate);

            rows.Add(new StatementRowDto(
                PaymentType,
                payment.Id,
                date,
                PersianDateFormatter.ToPersian(date),
                payment.Amount,
                null,
                null,
                null,
                null,
                null));
        }

        foreach (var check in checks)
        {
            var date = IranTime.ToIranDateTime(check.CreatedAt);

            rows.Add(new StatementRowDto(
                CheckType,
                check.Id,
                date,
                PersianDateFormatter.ToPersian(date),
                check.Amount,
                check.SettlementStatus.ToString(),
                check.CheckNumber,
                check.DueDate,
                PersianDateFormatter.ToPersian(check.DueDate),
                check.BankRegistration.ToString()));
        }

        var ordered = rows
            .OrderByDescending(x => x.Date)
            .ThenBy(x => x.Type, StringComparer.Ordinal)
            .ThenByDescending(x => x.Id)
            .ToList();

        return new CustomerStatementDto(
            customerId,
            customerName,
            from,
            to,
            BuildTotals(payments, checks),
            ordered,
            generatedAt);
    }

    public static StatementTotalsDto BuildTotals(
        IReadOnlyCollection<StatementPaymentRow> payments,
        IReadOnlyCollection<StatementCheckRow> checks)
    {
        var cash = payments.Sum(x => x.Amount);

        var pending = SumByStatus(checks, CheckSettlementStatus.Pending);
        var paid = SumByStatus(checks, CheckSettlementStatus.Paid);
        var bounced = SumByStatus(checks, CheckSettlementStatus.Bounced);

        return new StatementTotalsDto(
            cash,
            pending,
            paid,
            bounced,
            cash + paid,
            payments.Count,
            checks.Count);
    }

    private static decimal SumByStatus(
        IEnumerable<StatementCheckRow> checks,
        CheckSettlementStatus status)
        => checks
            .Where(x => x.SettlementStatus == status)
            .Sum(x => x.Amount);
}
