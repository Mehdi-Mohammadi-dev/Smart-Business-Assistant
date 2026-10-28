using Vexora.Application.Dtos.SubscriptionPlans;
using Vexora.Application.Exceptions;
using Vexora.Application.Exceptions.SubscriptionPlans;
using Vexora.Application.Repositories.SubscriptionPlans;
using Vexora.Application.Services.Interfaces.SubscriptionPlans;
using Vexora.Application.UnitOfWorks;
using Vexora.Domain.Entities.SubscriptionPlans;

namespace Vexora.Application.Services.Implementations.SubscriptionPlans;

public sealed class SubscriptionPlanService(
    ISubscriptionPlanRepository repository,
    IUnitOfWork unitOfWork) : ISubscriptionPlanService
{
    public async Task<IReadOnlyList<SubscriptionPlanDto>> GetActiveAsync(CancellationToken ct = default)
        => (await repository.ListAsync(true, ct)).Select(SubscriptionPlanDto.From).ToList();

    public async Task<IReadOnlyList<SubscriptionPlanDto>> GetAllAsync(CancellationToken ct = default)
        => (await repository.ListAsync(false, ct)).Select(SubscriptionPlanDto.From).ToList();

    public async Task<SubscriptionPlanDto> CreateAsync(SaveSubscriptionPlanDto dto, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(dto);
        SubscriptionPlan plan;
        try
        {
            plan = SubscriptionPlan.Create(dto.Name, dto.Days, dto.PriceToman,
                dto.DiscountPercent, dto.IsActive, dto.IsFeatured, dto.SortOrder);
        }
        catch (ArgumentException ex) { throw new InvalidSubscriptionPlanException(ex.Message); }

        await repository.AddAsync(plan, ct);
        await unitOfWork.SaveAsync(ct);
        return SubscriptionPlanDto.From(plan);
    }

    public async Task<SubscriptionPlanDto> UpdateAsync(long id, SaveSubscriptionPlanDto dto, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(dto);
        var plan = await repository.FindByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(SubscriptionPlan), id);
        try
        {
            plan.Update(dto.Name, dto.Days, dto.PriceToman,
                dto.DiscountPercent, dto.IsActive, dto.IsFeatured, dto.SortOrder);
        }
        catch (ArgumentException ex) { throw new InvalidSubscriptionPlanException(ex.Message); }

        await unitOfWork.SaveAsync(ct);
        return SubscriptionPlanDto.From(plan);
    }

    public async Task DeactivateAsync(long id, CancellationToken ct = default)
    {
        var plan = await repository.FindByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(SubscriptionPlan), id);
        plan.Deactivate();
        plan.SoftDelete();
        await unitOfWork.SaveAsync(ct);
    }
}
