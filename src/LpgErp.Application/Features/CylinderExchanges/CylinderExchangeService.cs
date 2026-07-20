using AutoMapper;
using LpgErp.Application.Common.Interfaces;
using LpgErp.Application.Common.Models;
using LpgErp.Application.Features.CylinderExchanges.DTOs;
using LpgErp.Domain.Entities;
using LpgErp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LpgErp.Application.Features.CylinderExchanges;

public interface ICylinderExchangeService
{
    Task<Result<PagedResult<CylinderExchangeDto>>> GetAllAsync(int pageNumber, int pageSize, CancellationToken ct = default);
    Task<Result<CylinderExchangeDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<CylinderExchangeDto>> CreateAsync(CreateCylinderExchangeRequest request, CancellationToken ct = default);
    Task<Result<CylinderExchangeDto>> UpdateAsync(Guid id, UpdateCylinderExchangeRequest request, CancellationToken ct = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken ct = default);
}

public class CylinderExchangeService : ICylinderExchangeService
{
    private readonly IApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CylinderExchangeService(IApplicationDbContext context, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<CylinderExchangeDto>>> GetAllAsync(int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var query = _context.CylinderExchanges.Where(e => !e.IsDeleted)
            .Include(e => e.Customer).Include(e => e.IncomingBrand).Include(e => e.IncomingCylinderSize)
            .Include(e => e.OutgoingBrand).Include(e => e.OutgoingCylinderSize)
            .OrderByDescending(e => e.ExchangeDate);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return Result<PagedResult<CylinderExchangeDto>>.Success(new PagedResult<CylinderExchangeDto>
        {
            Items = _mapper.Map<IReadOnlyList<CylinderExchangeDto>>(items),
            Pagination = new PaginationMeta { PageNumber = pageNumber, PageSize = pageSize, TotalCount = total }
        });
    }

    public async Task<Result<CylinderExchangeDto>> CreateAsync(CreateCylinderExchangeRequest request, CancellationToken ct = default)
    {
        var entity = _mapper.Map<CylinderExchange>(request);
        entity.ExchangeDate = DateTime.UtcNow;
        await _context.CylinderExchanges.AddAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var result = await _context.CylinderExchanges
            .Include(e => e.Customer).Include(e => e.IncomingBrand).Include(e => e.IncomingCylinderSize)
            .Include(e => e.OutgoingBrand).Include(e => e.OutgoingCylinderSize)
            .FirstOrDefaultAsync(e => e.Id == entity.Id, ct);
        return Result<CylinderExchangeDto>.Success(_mapper.Map<CylinderExchangeDto>(result));
    }

    public async Task<Result<CylinderExchangeDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _context.CylinderExchanges
            .Include(e => e.Customer).Include(e => e.IncomingBrand).Include(e => e.IncomingCylinderSize)
            .Include(e => e.OutgoingBrand).Include(e => e.OutgoingCylinderSize)
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted, ct);
        if (entity is null) return Result<CylinderExchangeDto>.Failure("Cylinder exchange not found.");
        return Result<CylinderExchangeDto>.Success(_mapper.Map<CylinderExchangeDto>(entity));
    }

    public async Task<Result<CylinderExchangeDto>> UpdateAsync(Guid id, UpdateCylinderExchangeRequest request, CancellationToken ct = default)
    {
        var entity = await _context.CylinderExchanges.FindAsync([id], ct);
        if (entity is null || entity.IsDeleted) return Result<CylinderExchangeDto>.Failure("Cylinder exchange not found.");

        _mapper.Map(request, entity);
        await _unitOfWork.SaveChangesAsync(ct);

        var result = await _context.CylinderExchanges
            .Include(e => e.Customer).Include(e => e.IncomingBrand).Include(e => e.IncomingCylinderSize)
            .Include(e => e.OutgoingBrand).Include(e => e.OutgoingCylinderSize)
            .FirstOrDefaultAsync(e => e.Id == entity.Id, ct);
        return Result<CylinderExchangeDto>.Success(_mapper.Map<CylinderExchangeDto>(result));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _context.CylinderExchanges.FindAsync([id], ct);
        if (entity is null || entity.IsDeleted) return Result.Failure("Cylinder exchange not found.");

        _context.CylinderExchanges.Remove(entity);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
