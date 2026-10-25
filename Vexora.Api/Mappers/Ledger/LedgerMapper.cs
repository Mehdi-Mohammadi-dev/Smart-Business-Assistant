using Vexora.Api.Contracts.Ledger;
using Vexora.Application.Common;
using Vexora.Application.Ledger.Checks;
using Vexora.Application.Ledger.Customers;
using Vexora.Application.Ledger.Payments;
using Vexora.Application.Ledger.Summary;

namespace Vexora.Api.Mappers.Ledger;

public static class LedgerMapper
{
    public static CustomerListQuery ToQuery(this CustomerListRequest request)
        => new(request.Search, request.Sort, request.Page, request.PageSize);

    public static PaymentListQuery ToQuery(this PaymentListRequest request)
        => new(request.CustomerId, request.From, request.To, request.Page, request.PageSize);

    public static CheckListQuery ToQuery(this CheckListRequest request)
        => new(
            request.CustomerId,
            request.Settlement,
            request.Registration,
            request.DueFrom,
            request.DueTo,
            request.Overdue,
            request.DueWithinDays,
            request.Search,
            request.Sort,
            request.Page,
            request.PageSize);

    public static PagedResult<TTarget> Map<TSource, TTarget>(
        this PagedResult<TSource> page,
        Func<TSource, TTarget> map)
        => new(
            page.Items.Select(map).ToList(),
            page.Page,
            page.PageSize,
            page.TotalCount);

    public static CustomerSummaryResponse ToResponse(this CustomerSummaryDto dto)
        => new(
            dto.Id,
            dto.FullName,
            dto.CashReceived,
            dto.ChecksPendingTotal,
            dto.ChecksPendingCount,
            dto.ChecksOverdueCount,
            dto.TotalReceived,
            dto.LastActivityAt);

    public static PaymentResponse ToResponse(this PaymentDto dto)
        => new(
            dto.Id,
            dto.CustomerId,
            dto.CustomerName,
            dto.Amount,
            dto.PaymentDate,
            PersianDateFormatter.ToPersian(IranTime.ToIranDate(dto.PaymentDate)),
            dto.Direction);

    public static CheckResponse ToResponse(this CheckDto dto)
        => new(
            dto.Id,
            dto.CustomerId,
            dto.CustomerName,
            dto.Amount,
            dto.DueDate,
            PersianDateFormatter.ToPersian(dto.DueDate),
            dto.CheckNumber,
            dto.Direction,
            dto.SettlementStatus,
            dto.BankRegistration,
            dto.PaidAt,
            dto.BouncedAt,
            dto.IsOverdue,
            dto.DaysToDue);

    public static SummaryResponse ToResponse(this LedgerSummaryDto dto)
        => new(
            dto.Today,
            dto.TodayPersian,
            dto.CustomersCount,
            new CheckSummaryResponse(
                dto.Checks.PendingCount,
                dto.Checks.PendingTotal,
                dto.Checks.OverdueCount,
                dto.Checks.OverdueTotal,
                dto.Checks.DueWithin7Count,
                dto.Checks.DueWithin7Total,
                dto.Checks.NotRegisteredPendingCount),
            new PaymentSummaryResponse(
                dto.Payments.TodayTotal,
                dto.Payments.MonthTotal,
                dto.Payments.MonthCount));
}
