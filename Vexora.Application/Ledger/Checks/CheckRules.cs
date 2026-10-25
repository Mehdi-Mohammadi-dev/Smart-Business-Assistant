using Vexora.Application.Common;
using Vexora.Application.Ledger.Models;
using Vexora.Domain.Enums.CheckSettlementStatuses;

namespace Vexora.Application.Ledger.Checks;

public static class CheckRules
{
    public static int? DaysToDue(
        CheckSettlementStatus status,
        DateTime dueDate,
        DateTime today)
        => status == CheckSettlementStatus.Pending
            ? (int)(dueDate.Date - today.Date).TotalDays
            : null;

    public static bool IsOverdue(int? daysToDue)
        => daysToDue is < 0;

    public static CheckDto ToDto(CheckRow row, DateTime today)
    {
        var daysToDue = DaysToDue(row.SettlementStatus, row.DueDate, today);

        return new CheckDto(
            row.Id,
            row.CustomerId,
            PersonName.Full(row.CustomerFirstName, row.CustomerLastName),
            row.Amount,
            row.DueDate,
            row.CheckNumber,
            row.Direction,
            row.SettlementStatus,
            row.BankRegistration,
            row.PaidAt,
            row.BouncedAt,
            IsOverdue(daysToDue),
            daysToDue);
    }
}
