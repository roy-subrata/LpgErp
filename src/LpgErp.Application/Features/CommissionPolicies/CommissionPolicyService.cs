using AutoMapper;
using LpgErp.Application.Common.Interfaces;
using LpgErp.Application.Common.Models;
using LpgErp.Application.Features.CommissionPolicies.DTOs;
using LpgErp.Domain.Entities;
using LpgErp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace LpgErp.Application.Features.CommissionPolicies;

public interface ICommissionPolicyService
{
    Task<Result<PagedResult<CommissionPolicyDto>>> GetAllAsync(int pageNumber, int pageSize, CancellationToken ct = default);
    Task<Result<IReadOnlyList<CommissionPolicyDto>>> GetByEntityAsync(CommissionEntityType entityType, Guid entityId, CancellationToken ct = default);
    Task<Result<CommissionPolicyDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<CommissionPolicyDto>> CreateAsync(CreateCommissionPolicyRequest request, CancellationToken ct = default);
    Task<Result<CommissionPolicyDto>> UpdateAsync(Guid id, UpdateCommissionPolicyRequest request, CancellationToken ct = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken ct = default);
}

public class CommissionPolicyService : ICommissionPolicyService
{
    private readonly IApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CommissionPolicyService(IApplicationDbContext context, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<CommissionPolicyDto>>> GetAllAsync(int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var query = _context.CommissionPolicies
            .Where(p => !p.IsDeleted)
            .Include(p => p.Product)
            .Include(p => p.Brand)
            .Include(p => p.CylinderSize)
            .OrderByDescending(p => p.CreatedAt);

        var total = await query.CountAsync(ct);
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        return Result<PagedResult<CommissionPolicyDto>>.Success(new PagedResult<CommissionPolicyDto>
        {
            Items = _mapper.Map<IReadOnlyList<CommissionPolicyDto>>(items),
            Pagination = new PaginationMeta { PageNumber = pageNumber, PageSize = pageSize, TotalCount = total }
        });
    }

    public async Task<Result<IReadOnlyList<CommissionPolicyDto>>> GetByEntityAsync(CommissionEntityType entityType, Guid entityId, CancellationToken ct = default)
    {
        var items = await _context.CommissionPolicies
            .Where(p => !p.IsDeleted && p.EntityType == entityType && p.EntityId == entityId)
            .Include(p => p.Product)
            .Include(p => p.Brand)
            .Include(p => p.CylinderSize)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);

        return Result<IReadOnlyList<CommissionPolicyDto>>.Success(_mapper.Map<IReadOnlyList<CommissionPolicyDto>>(items));
    }

    public async Task<Result<CommissionPolicyDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _context.CommissionPolicies
            .Include(p => p.Product)
            .Include(p => p.Brand)
            .Include(p => p.CylinderSize)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);

        if (entity is null) return Result<CommissionPolicyDto>.Failure("Commission policy not found.");
        return Result<CommissionPolicyDto>.Success(_mapper.Map<CommissionPolicyDto>(entity));
    }

    public async Task<Result<CommissionPolicyDto>> CreateAsync(CreateCommissionPolicyRequest request, CancellationToken ct = default)
    {
        if (request.TargetQuantity <= 0) return Result<CommissionPolicyDto>.Failure("Target quantity must be greater than zero.");
        if (request.CommissionValue <= 0) return Result<CommissionPolicyDto>.Failure("Commission value must be greater than zero.");

        var entity = _mapper.Map<CommissionPolicy>(request);
        await _context.CommissionPolicies.AddAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return await GetByIdAsync(entity.Id, ct);
    }

    public async Task<Result<CommissionPolicyDto>> UpdateAsync(Guid id, UpdateCommissionPolicyRequest request, CancellationToken ct = default)
    {
        var entity = await _context.CommissionPolicies.FindAsync([id], ct);
        if (entity is null || entity.IsDeleted) return Result<CommissionPolicyDto>.Failure("Commission policy not found.");

        _mapper.Map(request, entity);
        await _unitOfWork.SaveChangesAsync(ct);

        return await GetByIdAsync(id, ct);
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _context.CommissionPolicies.FindAsync([id], ct);
        if (entity is null || entity.IsDeleted) return Result.Failure("Commission policy not found.");

        _context.CommissionPolicies.Remove(entity);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }

    public static decimal CalculateCommission(CommissionPolicy policy, int actualQuantity, decimal actualAmount)
    {
        return policy.CalculationType switch
        {
            CommissionCalculationType.PerUnit => actualQuantity * policy.CommissionValue,
            CommissionCalculationType.FixedAmount => actualQuantity >= policy.TargetQuantity ? policy.CommissionValue : 0,
            CommissionCalculationType.Percentage => actualAmount * policy.CommissionValue / 100m,
            CommissionCalculationType.TargetBonus => actualQuantity >= policy.TargetQuantity ? policy.CommissionValue : 0,
            CommissionCalculationType.TieredPercentage => CalculateTiered(policy, actualQuantity, actualAmount),
            _ => 0
        };
    }

    private static decimal CalculateTiered(CommissionPolicy policy, int actualQuantity, decimal actualAmount)
    {
        if (string.IsNullOrEmpty(policy.TierConfig)) return 0;

        var tiers = JsonSerializer.Deserialize<List<CommissionTier>>(policy.TierConfig);
        if (tiers is null || tiers.Count == 0) return 0;

        var applicableTier = tiers.FirstOrDefault(t => actualQuantity >= t.MinQuantity && actualQuantity <= t.MaxQuantity);
        if (applicableTier is null) return 0;

        return actualAmount * applicableTier.CommissionPercent / 100m;
    }

    public static string GetPeriodKey(CommissionPeriodType periodType, DateTime date)
    {
        return periodType switch
        {
            CommissionPeriodType.Weekly => $"{date.Year}-W{System.Globalization.CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday):D2}",
            CommissionPeriodType.Monthly => $"{date.Year}-{date.Month:D2}",
            CommissionPeriodType.Yearly => $"{date.Year}",
            _ => "one-time"
        };
    }
}
