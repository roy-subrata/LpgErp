using AutoMapper;
using LpgErp.Application.Common.Interfaces;
using LpgErp.Application.Common.Models;
using LpgErp.Application.Features.DriverSettlements.DTOs;
using LpgErp.Domain.Entities;
using LpgErp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LpgErp.Application.Features.DriverSettlements;

public interface IDriverSettlementService
{
    Task<Result<PagedResult<DriverSettlementDto>>> GetAllAsync(int pageNumber, int pageSize, CancellationToken ct = default);
    Task<Result<DriverSettlementDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<DriverSettlementDto>> CreateAsync(CreateDriverSettlementRequest request, CancellationToken ct = default);
    Task<Result<DriverSettlementDto>> UpdateAsync(Guid id, UpdateDriverSettlementRequest request, CancellationToken ct = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken ct = default);
}

public class DriverSettlementService : IDriverSettlementService
{
    private readonly IApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public DriverSettlementService(IApplicationDbContext context, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<DriverSettlementDto>>> GetAllAsync(int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var query = _context.DriverSettlements.Where(s => !s.IsDeleted).Include(s => s.Driver).OrderByDescending(s => s.SettlementDate);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return Result<PagedResult<DriverSettlementDto>>.Success(new PagedResult<DriverSettlementDto>
        {
            Items = _mapper.Map<IReadOnlyList<DriverSettlementDto>>(items),
            Pagination = new PaginationMeta { PageNumber = pageNumber, PageSize = pageSize, TotalCount = total }
        });
    }

    public async Task<Result<DriverSettlementDto>> CreateAsync(CreateDriverSettlementRequest request, CancellationToken ct = default)
    {
        var entity = _mapper.Map<DriverSettlement>(request);
        entity.SettlementDate = DateTime.UtcNow;
        await _context.DriverSettlements.AddAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var result = await _context.DriverSettlements.Include(s => s.Driver).FirstOrDefaultAsync(s => s.Id == entity.Id, ct);
        return Result<DriverSettlementDto>.Success(_mapper.Map<DriverSettlementDto>(result));
    }

    public async Task<Result<DriverSettlementDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _context.DriverSettlements.Include(s => s.Driver).FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted, ct);
        if (entity is null) return Result<DriverSettlementDto>.Failure("Driver settlement not found.");
        return Result<DriverSettlementDto>.Success(_mapper.Map<DriverSettlementDto>(entity));
    }

    public async Task<Result<DriverSettlementDto>> UpdateAsync(Guid id, UpdateDriverSettlementRequest request, CancellationToken ct = default)
    {
        var entity = await _context.DriverSettlements.FindAsync([id], ct);
        if (entity is null || entity.IsDeleted) return Result<DriverSettlementDto>.Failure("Driver settlement not found.");

        _mapper.Map(request, entity);
        await _unitOfWork.SaveChangesAsync(ct);

        var result = await _context.DriverSettlements.Include(s => s.Driver).FirstOrDefaultAsync(s => s.Id == entity.Id, ct);
        return Result<DriverSettlementDto>.Success(_mapper.Map<DriverSettlementDto>(result));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _context.DriverSettlements.FindAsync([id], ct);
        if (entity is null || entity.IsDeleted) return Result.Failure("Driver settlement not found.");

        _context.DriverSettlements.Remove(entity);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
