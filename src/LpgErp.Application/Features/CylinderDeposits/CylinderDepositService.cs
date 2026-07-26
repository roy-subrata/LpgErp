using AutoMapper;
using LpgErp.Application.Common.Interfaces;
using LpgErp.Application.Common.Models;
using LpgErp.Application.Features.CylinderDeposits.DTOs;
using LpgErp.Domain.Entities;
using LpgErp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LpgErp.Application.Features.CylinderDeposits;

public interface ICylinderDepositService
{
    Task<Result<PagedResult<CylinderDepositDto>>> GetAllAsync(int pageNumber, int pageSize, CancellationToken ct = default);
    Task<Result<CylinderDepositDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<CylinderDepositDto>> CreateAsync(CreateCylinderDepositRequest request, CancellationToken ct = default);
    Task<Result<CylinderDepositDto>> UpdateAsync(Guid id, UpdateCylinderDepositRequest request, CancellationToken ct = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken ct = default);
}

public class CylinderDepositService : ICylinderDepositService
{
    private readonly IApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CylinderDepositService(IApplicationDbContext context, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<CylinderDepositDto>>> GetAllAsync(int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var query = _context.CylinderDeposits.Where(d => !d.IsDeleted).Include(d => d.Customer).Include(d => d.CylinderSize).OrderByDescending(d => d.DepositDate);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return Result<PagedResult<CylinderDepositDto>>.Success(new PagedResult<CylinderDepositDto>
        {
            Items = _mapper.Map<IReadOnlyList<CylinderDepositDto>>(items),
            Pagination = new PaginationMeta { PageNumber = pageNumber, PageSize = pageSize, TotalCount = total }
        });
    }

    public async Task<Result<CylinderDepositDto>> CreateAsync(CreateCylinderDepositRequest request, CancellationToken ct = default)
    {
        if (request.Amount <= 0) return Result<CylinderDepositDto>.Failure("Amount must be greater than zero.");
        if (request.Quantity <= 0) return Result<CylinderDepositDto>.Failure("Quantity must be greater than zero.");

        if (request.Type == CylinderDepositType.Refund)
        {
            var totalPaid = await _context.CylinderDeposits
                .Where(d => !d.IsDeleted && d.CustomerId == request.CustomerId && d.CylinderSizeId == request.CylinderSizeId && d.Type == CylinderDepositType.Paid)
                .SumAsync(d => d.Amount, ct);
            var totalRefunded = await _context.CylinderDeposits
                .Where(d => !d.IsDeleted && d.CustomerId == request.CustomerId && d.CylinderSizeId == request.CylinderSizeId && d.Type == CylinderDepositType.Refund)
                .SumAsync(d => d.Amount, ct);
            if (totalPaid - totalRefunded < request.Amount)
                return Result<CylinderDepositDto>.Failure($"Refund amount ({request.Amount:N2}) exceeds available deposit balance ({totalPaid - totalRefunded:N2}).");
        }

        var entity = _mapper.Map<CylinderDeposit>(request);
        entity.DepositDate = DateTime.UtcNow;
        await _context.CylinderDeposits.AddAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var result = await _context.CylinderDeposits.Include(d => d.Customer).Include(d => d.CylinderSize).FirstOrDefaultAsync(d => d.Id == entity.Id, ct);
        return Result<CylinderDepositDto>.Success(_mapper.Map<CylinderDepositDto>(result));
    }

    public async Task<Result<CylinderDepositDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _context.CylinderDeposits.Include(d => d.Customer).Include(d => d.CylinderSize).FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted, ct);
        if (entity is null) return Result<CylinderDepositDto>.Failure("Cylinder deposit not found.");
        return Result<CylinderDepositDto>.Success(_mapper.Map<CylinderDepositDto>(entity));
    }

    public async Task<Result<CylinderDepositDto>> UpdateAsync(Guid id, UpdateCylinderDepositRequest request, CancellationToken ct = default)
    {
        var entity = await _context.CylinderDeposits.FindAsync([id], ct);
        if (entity is null || entity.IsDeleted) return Result<CylinderDepositDto>.Failure("Cylinder deposit not found.");

        if (request.Amount <= 0) return Result<CylinderDepositDto>.Failure("Amount must be greater than zero.");
        if (request.Quantity <= 0) return Result<CylinderDepositDto>.Failure("Quantity must be greater than zero.");
        if (request.Type != entity.Type) return Result<CylinderDepositDto>.Failure("Deposit type cannot be changed. Create a new entry instead.");
        if (request.CustomerId != entity.CustomerId) return Result<CylinderDepositDto>.Failure("Customer cannot be changed on an existing deposit.");

        entity.Amount = request.Amount;
        entity.Quantity = request.Quantity;
        entity.Reference = request.Reference;
        entity.Notes = request.Notes;
        await _unitOfWork.SaveChangesAsync(ct);

        var result = await _context.CylinderDeposits.Include(d => d.Customer).Include(d => d.CylinderSize).FirstOrDefaultAsync(d => d.Id == entity.Id, ct);
        return Result<CylinderDepositDto>.Success(_mapper.Map<CylinderDepositDto>(result));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _context.CylinderDeposits.FindAsync([id], ct);
        if (entity is null || entity.IsDeleted) return Result.Failure("Cylinder deposit not found.");

        _context.CylinderDeposits.Remove(entity);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
