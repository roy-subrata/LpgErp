using AutoMapper;
using LpgErp.Application.Common.Interfaces;
using LpgErp.Application.Common.Models;
using LpgErp.Application.Features.VehicleLoadings.DTOs;
using LpgErp.Domain.Entities;
using LpgErp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LpgErp.Application.Features.VehicleClosings;

public interface IVehicleClosingService
{
    Task<Result<PagedResult<VehicleClosingDto>>> GetAllAsync(int pageNumber, int pageSize, CancellationToken ct = default);
    Task<Result<VehicleClosingDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
}

public class VehicleClosingService : IVehicleClosingService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public VehicleClosingService(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<VehicleClosingDto>>> GetAllAsync(int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var query = _context.VehicleClosings
            .Where(vc => !vc.IsDeleted)
            .Include(vc => vc.VehicleLoading).ThenInclude(vl => vl.Truck)
            .Include(vc => vc.VehicleLoading).ThenInclude(vl => vl.Driver)
            .Include(vc => vc.VehicleLoading).ThenInclude(vl => vl.Salesman)
            .Include(vc => vc.Items).ThenInclude(i => i.Product)
            .OrderByDescending(vc => vc.ClosingDate);

        var total = await query.CountAsync(ct);
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        return Result<PagedResult<VehicleClosingDto>>.Success(new PagedResult<VehicleClosingDto>
        {
            Items = _mapper.Map<IReadOnlyList<VehicleClosingDto>>(items),
            Pagination = new PaginationMeta { PageNumber = pageNumber, PageSize = pageSize, TotalCount = total }
        });
    }

    public async Task<Result<VehicleClosingDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _context.VehicleClosings
            .Include(vc => vc.VehicleLoading).ThenInclude(vl => vl.Truck)
            .Include(vc => vc.VehicleLoading).ThenInclude(vl => vl.Driver)
            .Include(vc => vc.VehicleLoading).ThenInclude(vl => vl.Salesman)
            .Include(vc => vc.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(vc => vc.Id == id && !vc.IsDeleted, ct);

        if (entity is null) return Result<VehicleClosingDto>.Failure("Vehicle closing not found.");
        return Result<VehicleClosingDto>.Success(_mapper.Map<VehicleClosingDto>(entity));
    }
}
