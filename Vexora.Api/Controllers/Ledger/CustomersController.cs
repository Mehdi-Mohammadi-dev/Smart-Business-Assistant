using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vexora.Api.Commons.Results;
using Vexora.Api.Contracts.Ledger;
using Vexora.Api.Mappers.Ledger;
using Vexora.Application.Common;
using Vexora.Application.Ledger.Customers;
using Vexora.Application.Ledger.Statements;

namespace Vexora.Api.Controllers.Ledger;

[Authorize]
[ApiController]
[Route("api/customers")]
public sealed class CustomersController(
    ILedgerCustomerService customerService,
    IStatementService statementService)
    : ControllerBase
{
    [HttpGet]
    [EndpointSummary("فهرست مشتری‌ها")]
    [EndpointDescription("مشتری‌ها با جمع دریافتی و چک‌ها. sort: LastActivity (پیش‌فرض) یا Name. pageSize حداکثر ۱۰۰.")]
    [ProducesResponseType<ApiResult<PagedResult<CustomerSummaryResponse>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResult<object>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiResult<object>>(StatusCodes.Status401Unauthorized)]
    public async Task<ApiResult<PagedResult<CustomerSummaryResponse>>> List(
        [FromQuery] CustomerListRequest request,
        CancellationToken cancellationToken)
    {
        var page = await customerService.ListAsync(request.ToQuery(), cancellationToken);

        return ApiResult<PagedResult<CustomerSummaryResponse>>.SuccessResult(
            page.Map(x => x.ToResponse()));
    }

    [HttpGet("{id:long}")]
    [EndpointSummary("خلاصه یک مشتری")]
    [EndpointDescription("جمع دریافتی و وضعیت چک‌های یک مشتری.")]
    [ProducesResponseType<ApiResult<CustomerSummaryResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResult<object>>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiResult<object>>(StatusCodes.Status404NotFound)]
    public async Task<ApiResult<CustomerSummaryResponse>> Get(
        long id,
        CancellationToken cancellationToken)
    {
        var customer = await customerService.GetAsync(id, cancellationToken);

        return ApiResult<CustomerSummaryResponse>.SuccessResult(customer.ToResponse());
    }

    [HttpGet("{id:long}/statement")]
    [EndpointSummary("حساب مشتری")]
    [EndpointDescription("گردش دریافت‌ها و چک‌های مشتری. from و to اختیاری‌اند؛ برای پرداخت روی تاریخ پرداخت و برای چک روی سررسید. سقف ۵۰۰۰ ردیف.")]
    [ProducesResponseType<ApiResult<CustomerStatementDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiResult<object>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiResult<object>>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiResult<object>>(StatusCodes.Status404NotFound)]
    public async Task<ApiResult<CustomerStatementDto>> GetStatement(
        long id,
        [FromQuery] StatementRequest request,
        CancellationToken cancellationToken)
    {
        var statement = await statementService.GetAsync(
            id,
            request.From,
            request.To,
            cancellationToken);

        return ApiResult<CustomerStatementDto>.SuccessResult(statement);
    }
}
