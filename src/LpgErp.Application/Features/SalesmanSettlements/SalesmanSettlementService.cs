using AutoMapper;
using LpgErp.Application.Common.Interfaces;
using LpgErp.Application.Common.Models;
using LpgErp.Application.Features.SalesmanSettlements.DTOs;
using LpgErp.Domain.Entities;
using LpgErp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LpgErp.Application.Features.SalesmanSettlements;

public interface ISalesmanSettlementService
{
    Task<Result<PagedResult<SalesmanSettlementDto>>> GetAllAsync(int pageNumber, int pageSize, CancellationToken ct = default);
    Task<Result<SalesmanSettlementDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<SalesmanSettlementDto>> CreateAsync(CreateSalesmanSettlementRequest request, CancellationToken ct = default);
    Task<Result<SalesmanSettlementDto>> UpdateAsync(Guid id, UpdateSalesmanSettlementRequest request, CancellationToken ct = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken ct = default);
}

public class SalesmanSettlementService : ISalesmanSettlementService
{
    private readonly IApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SalesmanSettlementService(IApplicationDbContext context, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<SalesmanSettlementDto>>> GetAllAsync(int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var query = _context.SalesmanSettlements.Where(s => !s.IsDeleted).Include(s => s.Salesman).OrderByDescending(s => s.SettlementDate);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return Result<PagedResult<SalesmanSettlementDto>>.Success(new PagedResult<SalesmanSettlementDto>
        {
            Items = _mapper.Map<IReadOnlyList<SalesmanSettlementDto>>(items),
            Pagination = new PaginationMeta { PageNumber = pageNumber, PageSize = pageSize, TotalCount = total }
        });
    }

    public async Task<Result<SalesmanSettlementDto>> CreateAsync(CreateSalesmanSettlementRequest request, CancellationToken ct = default)
    {
        var entity = _mapper.Map<SalesmanSettlement>(request);
        entity.SettlementDate = DateTime.UtcNow;
        await _context.SalesmanSettlements.AddAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var result = await _context.SalesmanSettlements.Include(s => s.Salesman).FirstOrDefaultAsync(s => s.Id == entity.Id, ct);
        return Result<SalesmanSettlementDto>.Success(_mapper.Map<SalesmanSettlementDto>(result));
    }

    public async Task<Result<SalesmanSettlementDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _context.SalesmanSettlements.Include(s => s.Salesman).FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted, ct);
        if (entity is null) return Result<SalesmanSettlementDto>.Failure("Salesman settlement not found.");
        return Result<SalesmanSettlementDto>.Success(_mapper.Map<SalesmanSettlementDto>(entity));
    }

    public async Task<Result<SalesmanSettlementDto>> UpdateAsync(Guid id, UpdateSalesmanSettlementRequest request, CancellationToken ct = default)
    {
        var entity = await _context.SalesmanSettlements.FindAsync([id], ct);
        if (entity is null || entity.IsDeleted) return Result<SalesmanSettlementDto>.Failure("Salesman settlement not found.");

        _mapper.Map(request, entity);
        await _unitOfWork.SaveChangesAsync(ct);

        var result = await _context.SalesmanSettlements.Include(s => s.Salesman).FirstOrDefaultAsync(s => s.Id == entity.Id, ct);
        return Result<SalesmanSettlementDto>.Success(_mapper.Map<SalesmanSettlementDto>(result));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _context.SalesmanSettlements.FindAsync([id], ct);
        if (entity is null || entity.IsDeleted) return Result.Failure("Salesman settlement not found.");

        _context.SalesmanSettlements.Remove(entity);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
