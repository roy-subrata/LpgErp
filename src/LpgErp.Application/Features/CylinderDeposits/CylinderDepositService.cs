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

        _mapper.Map(request, entity);
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
