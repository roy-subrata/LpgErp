using AutoMapper;
using LpgErp.Application.Common.Interfaces;
using LpgErp.Application.Common.Models;
using LpgErp.Application.Features.CommissionLedgers.DTOs;
using LpgErp.Application.Features.CommissionPolicies;
using LpgErp.Domain.Entities;
using LpgErp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LpgErp.Application.Features.CommissionLedgers;

public interface ICommissionLedgerService
{
    Task<Result<PagedResult<CommissionLedgerDto>>> GetAllAsync(int pageNumber, int pageSize, CancellationToken ct = default);
    Task<Result<IReadOnlyList<CommissionLedgerDto>>> GetByEntityAsync(CommissionEntityType entityType, Guid entityId, CancellationToken ct = default);
    Task<Result<CommissionLedgerDto>> CalculateAsync(CalculateCommissionRequest request, CancellationToken ct = default);
    Task<Result<CommissionLedgerDto>> SettleAsync(SettleCommissionRequest request, CancellationToken ct = default);
    Task CalculateAndRecordForPeriodAsync(CommissionEntityType entityType, Guid entityId, DateTime date, CancellationToken ct = default);
}

public class CommissionLedgerService : ICommissionLedgerService
{
    private readonly IApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CommissionLedgerService(IApplicationDbContext context, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<CommissionLedgerDto>>> GetAllAsync(int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var query = _context.CommissionLedgers
            .Where(l => !l.IsDeleted)
            .Include(l => l.Policy)
            .OrderByDescending(l => l.CreatedAt);

        var total = await query.CountAsync(ct);
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        return Result<PagedResult<CommissionLedgerDto>>.Success(new PagedResult<CommissionLedgerDto>
        {
            Items = _mapper.Map<IReadOnlyList<CommissionLedgerDto>>(items),
            Pagination = new PaginationMeta { PageNumber = pageNumber, PageSize = pageSize, TotalCount = total }
        });
    }

    public async Task<Result<IReadOnlyList<CommissionLedgerDto>>> GetByEntityAsync(CommissionEntityType entityType, Guid entityId, CancellationToken ct = default)
    {
        var items = await _context.CommissionLedgers
            .Where(l => !l.IsDeleted && l.EntityType == entityType && l.EntityId == entityId)
            .Include(l => l.Policy)
            .OrderByDescending(l => l.PeriodStart)
            .ToListAsync(ct);

        return Result<IReadOnlyList<CommissionLedgerDto>>.Success(_mapper.Map<IReadOnlyList<CommissionLedgerDto>>(items));
    }

    public async Task<Result<CommissionLedgerDto>> CalculateAsync(CalculateCommissionRequest request, CancellationToken ct = default)
    {
        var policy = await _context.CommissionPolicies
            .Include(p => p.Product)
            .FirstOrDefaultAsync(p => p.Id == request.PolicyId && !p.IsDeleted, ct);

        if (policy is null) return Result<CommissionLedgerDto>.Failure("Commission policy not found.");
        if (!policy.IsActive) return Result<CommissionLedgerDto>.Failure("Commission policy is not active.");

        var periodKey = request.PeriodKey ?? CommissionPolicyService.GetPeriodKey(policy.PeriodType, DateTime.UtcNow);
        var periodStart = GetPeriodStart(policy.PeriodType, DateTime.UtcNow);
        var periodEnd = GetPeriodEnd(policy.PeriodType, DateTime.UtcNow);

        var existing = await _context.CommissionLedgers
            .FirstOrDefaultAsync(l => !l.IsDeleted && l.PolicyId == policy.Id && l.EntityType == policy.EntityType && l.EntityId == policy.EntityId && l.PeriodKey == periodKey, ct);

        if (existing is not null)
            return Result<CommissionLedgerDto>.Failure($"Commission already calculated for this period ({periodKey}).");

        var (actualQuantity, actualAmount) = await GetActualsForPeriodAsync(policy, periodStart, periodEnd, ct);

        var commissionEarned = CommissionPolicyService.CalculateCommission(policy, actualQuantity, actualAmount);

        var ledger = new CommissionLedger
        {
            PolicyId = policy.Id,
            EntityType = policy.EntityType,
            EntityId = policy.EntityId,
            PeriodKey = periodKey,
            ActualQuantity = actualQuantity,
            ActualAmount = actualAmount,
            CommissionEarned = commissionEarned,
            Status = CommissionLedgerStatus.Earned,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd
        };

        await _context.CommissionLedgers.AddAsync(ledger, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var result = await _context.CommissionLedgers.Include(l => l.Policy).FirstOrDefaultAsync(l => l.Id == ledger.Id, ct);
        return Result<CommissionLedgerDto>.Success(_mapper.Map<CommissionLedgerDto>(result));
    }

    public async Task<Result<CommissionLedgerDto>> SettleAsync(SettleCommissionRequest request, CancellationToken ct = default)
    {
        var ledger = await _context.CommissionLedgers
            .Include(l => l.Policy)
            .FirstOrDefaultAsync(l => l.Id == request.LedgerId && !l.IsDeleted, ct);

        if (ledger is null) return Result<CommissionLedgerDto>.Failure("Commission ledger entry not found.");
        if (ledger.Status != CommissionLedgerStatus.Earned)
            return Result<CommissionLedgerDto>.Failure("Only earned commissions can be settled.");

        ledger.Status = CommissionLedgerStatus.Applied;
        ledger.AppliedDate = DateTime.UtcNow;
        ledger.Reference = request.Reference;

        await _unitOfWork.SaveChangesAsync(ct);

        return Result<CommissionLedgerDto>.Success(_mapper.Map<CommissionLedgerDto>(ledger));
    }

    public async Task CalculateAndRecordForPeriodAsync(CommissionEntityType entityType, Guid entityId, DateTime date, CancellationToken ct = default)
    {
        var policies = await _context.CommissionPolicies
            .Where(p => !p.IsDeleted && p.IsActive && p.EntityType == entityType && p.EntityId == entityId
                && p.StartDate <= date && (p.EndDate == null || p.EndDate >= date))
            .Include(p => p.Product)
            .ToListAsync(ct);

        var periodKey = CommissionPolicyService.GetPeriodKey(
            policies.FirstOrDefault()?.PeriodType ?? CommissionPeriodType.Monthly, date);

        foreach (var policy in policies)
        {
            var existing = await _context.CommissionLedgers
                .AnyAsync(l => !l.IsDeleted && l.PolicyId == policy.Id && l.EntityType == entityType && l.EntityId == entityId && l.PeriodKey == periodKey, ct);

            if (existing) continue;

            var periodStart = GetPeriodStart(policy.PeriodType, date);
            var periodEnd = GetPeriodEnd(policy.PeriodType, date);
            var (actualQuantity, actualAmount) = await GetActualsForPeriodAsync(policy, periodStart, periodEnd, ct);
            var commissionEarned = CommissionPolicyService.CalculateCommission(policy, actualQuantity, actualAmount);

            if (commissionEarned > 0)
            {
                await _context.CommissionLedgers.AddAsync(new CommissionLedger
                {
                    PolicyId = policy.Id,
                    EntityType = entityType,
                    EntityId = entityId,
                    PeriodKey = periodKey,
                    ActualQuantity = actualQuantity,
                    ActualAmount = actualAmount,
                    CommissionEarned = commissionEarned,
                    Status = CommissionLedgerStatus.Earned,
                    PeriodStart = periodStart,
                    PeriodEnd = periodEnd
                }, ct);
            }
        }

        await _unitOfWork.SaveChangesAsync(ct);
    }

    private async Task<(int quantity, decimal amount)> GetActualsForPeriodAsync(CommissionPolicy policy, DateTime periodStart, DateTime periodEnd, CancellationToken ct)
    {
        if (policy.EntityType == CommissionEntityType.Salesman)
        {
            return await GetSalesmanActualsAsync(policy, periodStart, periodEnd, ct);
        }
        else if (policy.EntityType == CommissionEntityType.Customer)
        {
            return await GetCustomerActualsAsync(policy, periodStart, periodEnd, ct);
        }
        else if (policy.EntityType == CommissionEntityType.Supplier)
        {
            return await GetSupplierActualsAsync(policy, periodStart, periodEnd, ct);
        }

        return (0, 0);
    }

    private async Task<(int quantity, decimal amount)> GetSalesmanActualsAsync(CommissionPolicy policy, DateTime start, DateTime end, CancellationToken ct)
    {
        var query = _context.SalesOrders
            .Where(so => !so.IsDeleted && so.Status == SalesOrderStatus.Delivered
                && so.OrderDate >= start && so.OrderDate <= end);

        if (policy.ProductId.HasValue)
        {
            query = query.Where(so => so.Items.Any(i => i.ProductId == policy.ProductId.Value));
        }
        else if (policy.BrandId.HasValue || policy.CylinderSizeId.HasValue)
        {
            query = query.Where(so => so.Items.Any(i =>
                i.Product.BrandId == policy.BrandId || i.Product.CylinderSizeId == policy.CylinderSizeId));
        }

        var items = await query.SelectMany(so => so.Items).ToListAsync(ct);

        if (policy.ProductId.HasValue)
            items = items.Where(i => i.ProductId == policy.ProductId.Value).ToList();
        else if (policy.BrandId.HasValue || policy.CylinderSizeId.HasValue)
            items = items.Where(i => i.Product.BrandId == policy.BrandId || i.Product.CylinderSizeId == policy.CylinderSizeId).ToList();

        return (items.Sum(i => i.Quantity), items.Sum(i => i.TotalPrice));
    }

    private async Task<(int quantity, decimal amount)> GetCustomerActualsAsync(CommissionPolicy policy, DateTime start, DateTime end, CancellationToken ct)
    {
        var query = _context.SalesOrders
            .Where(so => !so.IsDeleted && so.Status == SalesOrderStatus.Delivered && so.CustomerId == policy.EntityId
                && so.OrderDate >= start && so.OrderDate <= end);

        if (policy.ProductId.HasValue)
            query = query.Where(so => so.Items.Any(i => i.ProductId == policy.ProductId.Value));

        var items = await query.SelectMany(so => so.Items).ToListAsync(ct);

        if (policy.ProductId.HasValue)
            items = items.Where(i => i.ProductId == policy.ProductId.Value).ToList();

        return (items.Sum(i => i.Quantity), items.Sum(i => i.TotalPrice));
    }

    private async Task<(int quantity, decimal amount)> GetSupplierActualsAsync(CommissionPolicy policy, DateTime start, DateTime end, CancellationToken ct)
    {
        var query = _context.PurchaseOrders
            .Where(po => !po.IsDeleted && po.SupplierId == policy.EntityId
                && po.OrderDate >= start && po.OrderDate <= end);

        if (policy.ProductId.HasValue)
            query = query.Where(po => po.Items.Any(i => i.ProductId == policy.ProductId.Value));

        var items = await query.SelectMany(po => po.Items).ToListAsync(ct);

        if (policy.ProductId.HasValue)
            items = items.Where(i => i.ProductId == policy.ProductId.Value).ToList();

        return (items.Sum(i => i.ReceivedQuantity), items.Sum(i => i.TotalPrice));
    }

    private static DateTime GetPeriodStart(CommissionPeriodType periodType, DateTime date)
    {
        return periodType switch
        {
            CommissionPeriodType.Weekly => date.AddDays(-(int)date.DayOfWeek + 1),
            CommissionPeriodType.Monthly => new DateTime(date.Year, date.Month, 1),
            CommissionPeriodType.Yearly => new DateTime(date.Year, 1, 1),
            _ => date
        };
    }

    private static DateTime GetPeriodEnd(CommissionPeriodType periodType, DateTime date)
    {
        return periodType switch
        {
            CommissionPeriodType.Weekly => date.AddDays(7 - (int)date.DayOfWeek),
            CommissionPeriodType.Monthly => new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month)),
            CommissionPeriodType.Yearly => new DateTime(date.Year, 12, 31),
            _ => date
        };
    }
}
