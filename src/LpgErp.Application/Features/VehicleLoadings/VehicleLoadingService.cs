using AutoMapper;
using LpgErp.Application.Common.Interfaces;
using LpgErp.Application.Common.Models;
using LpgErp.Application.Features.VehicleLoadings.DTOs;
using LpgErp.Domain.Entities;
using LpgErp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LpgErp.Application.Features.VehicleLoadings;

public interface IVehicleLoadingService
{
    Task<Result<PagedResult<VehicleLoadingDto>>> GetAllAsync(int pageNumber, int pageSize, CancellationToken ct = default);
    Task<Result<VehicleLoadingDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<VehicleLoadingDto>> CreateAsync(CreateVehicleLoadingRequest request, CancellationToken ct = default);
    Task<Result<VehicleClosingDto>> CloseAsync(Guid loadingId, CreateVehicleClosingRequest request, CancellationToken ct = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken ct = default);
}

public class VehicleLoadingService : IVehicleLoadingService
{
    private readonly IApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public VehicleLoadingService(IApplicationDbContext context, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<VehicleLoadingDto>>> GetAllAsync(int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var query = _context.VehicleLoadings
            .Where(v => !v.IsDeleted)
            .Include(v => v.Truck).Include(v => v.Driver).Include(v => v.Salesman).Include(v => v.Warehouse).Include(v => v.Route)
            .OrderByDescending(v => v.LoadingDate);

        var total = await query.CountAsync(ct);
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        return Result<PagedResult<VehicleLoadingDto>>.Success(new PagedResult<VehicleLoadingDto>
        {
            Items = _mapper.Map<IReadOnlyList<VehicleLoadingDto>>(items),
            Pagination = new PaginationMeta { PageNumber = pageNumber, PageSize = pageSize, TotalCount = total }
        });
    }

    public async Task<Result<VehicleLoadingDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _context.VehicleLoadings
            .Include(v => v.Truck).Include(v => v.Driver).Include(v => v.Salesman).Include(v => v.Warehouse).Include(v => v.Route)
            .Include(v => v.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted, ct);

        if (entity is null) return Result<VehicleLoadingDto>.Failure("Vehicle loading not found.");
        return Result<VehicleLoadingDto>.Success(_mapper.Map<VehicleLoadingDto>(entity));
    }

    public async Task<Result<VehicleLoadingDto>> CreateAsync(CreateVehicleLoadingRequest request, CancellationToken ct = default)
    {
        var loading = new VehicleLoading
        {
            LoadingDate = request.LoadingDate,
            TruckId = request.TruckId,
            DriverId = request.DriverId,
            SalesmanId = request.SalesmanId,
            WarehouseId = request.WarehouseId,
            RouteId = request.RouteId,
            Notes = request.Notes,
            Status = VehicleLoadingStatus.Dispatched,
            Items = request.Items.Select(i => new VehicleLoadingItem
            {
                ProductId = i.ProductId,
                LoadedQuantity = i.LoadedQuantity
            }).ToList()
        };

        await _context.VehicleLoadings.AddAsync(loading, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return await GetByIdAsync(loading.Id, ct);
    }

    public async Task<Result<VehicleClosingDto>> CloseAsync(Guid loadingId, CreateVehicleClosingRequest request, CancellationToken ct = default)
    {
        var loading = await _context.VehicleLoadings.FindAsync([loadingId], ct);
        if (loading is null) return Result<VehicleClosingDto>.Failure("Vehicle loading not found.");

        var closing = new VehicleClosing
        {
            VehicleLoadingId = loadingId,
            ClosingDate = DateTime.UtcNow,
            CashCollected = request.CashCollected,
            CreditSales = request.CreditSales,
            OutstandingAmount = request.OutstandingAmount,
            CylinderExchanges = request.CylinderExchanges,
            ReturnedEmptyCylinders = request.ReturnedEmptyCylinders,
            DamagedCount = request.DamagedCount,
            LeakageCount = request.LeakageCount,
            Variance = request.Variance,
            Notes = request.Notes,
            Items = request.Items.Select(i => new VehicleClosingItem
            {
                ProductId = i.ProductId,
                LoadedQuantity = i.LoadedQuantity,
                SoldQuantity = i.SoldQuantity,
                ReturnedQuantity = i.ReturnedQuantity,
                DamagedQuantity = i.DamagedQuantity
            }).ToList()
        };

        loading.Status = VehicleLoadingStatus.Returned;

        await _context.VehicleClosings.AddAsync(closing, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var result = await _context.VehicleClosings
            .Include(vc => vc.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(vc => vc.Id == closing.Id, ct);

        return Result<VehicleClosingDto>.Success(_mapper.Map<VehicleClosingDto>(result));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _context.VehicleLoadings.FindAsync([id], ct);
        if (entity is null) return Result.Failure("Vehicle loading not found.");
        if (entity.Status != VehicleLoadingStatus.Dispatched) return Result.Failure("Only dispatched vehicle loadings can be deleted.");

        _context.VehicleLoadings.Remove(entity);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
