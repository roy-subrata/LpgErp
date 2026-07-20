using AutoMapper;
using LpgErp.Application.Common.Interfaces;
using LpgErp.Application.Common.Models;
using LpgErp.Domain.Interfaces;
using LpgErp.Application.Features.DailySalesSummaries.DTOs;
using LpgErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LpgErp.Application.Features.DailySalesSummaries;

public interface IDailySalesSummaryService
{
    Task<Result<PagedResult<DailySalesSummaryDto>>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<Result<DailySalesSummaryDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<DailySalesSummaryDto>> CreateAsync(CreateDailySalesSummaryRequest request, CancellationToken cancellationToken = default);
    Task<Result<DailySalesSummaryDto>> UpdateAsync(Guid id, UpdateDailySalesSummaryRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public class DailySalesSummaryService : IDailySalesSummaryService
{
    private readonly IApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public DailySalesSummaryService(IApplicationDbContext context, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<DailySalesSummaryDto>>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.DailySalesSummaries
            .Where(d => !d.IsDeleted)
            .Include(d => d.Truck)
            .Include(d => d.Driver)
            .Include(d => d.Salesman)
            .Include(d => d.VehicleLoading)
            .OrderByDescending(d => d.SummaryDate);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return Result<PagedResult<DailySalesSummaryDto>>.Success(new PagedResult<DailySalesSummaryDto>
        {
            Items = _mapper.Map<IReadOnlyList<DailySalesSummaryDto>>(items),
            Pagination = new PaginationMeta { PageNumber = pageNumber, PageSize = pageSize, TotalCount = totalCount }
        });
    }

    public async Task<Result<DailySalesSummaryDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.DailySalesSummaries
            .Include(d => d.Truck)
            .Include(d => d.Driver)
            .Include(d => d.Salesman)
            .Include(d => d.VehicleLoading)
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted, cancellationToken);

        if (entity is null) return Result<DailySalesSummaryDto>.Failure("Daily sales summary not found.");
        return Result<DailySalesSummaryDto>.Success(_mapper.Map<DailySalesSummaryDto>(entity));
    }

    public async Task<Result<DailySalesSummaryDto>> CreateAsync(CreateDailySalesSummaryRequest request, CancellationToken cancellationToken = default)
    {
        var entity = new DailySalesSummary
        {
            SummaryDate = DateTime.UtcNow,
            VehicleLoadingId = request.VehicleLoadingId,
            TruckId = request.TruckId,
            DriverId = request.DriverId,
            SalesmanId = request.SalesmanId,
            TotalSales = request.TotalSales,
            CashSales = request.CashSales,
            CreditSales = request.CreditSales,
            PackagesSold = request.PackagesSold,
            RefillsSold = request.RefillsSold,
            EmptyCylindersSold = request.EmptyCylindersSold,
            AccessoriesSold = request.AccessoriesSold,
            PaymentsCollected = request.PaymentsCollected,
            DueCreated = request.DueCreated,
            CylinderBalance = request.CylinderBalance,
            OutstandingCylinders = request.OutstandingCylinders,
            StockReturned = request.StockReturned,
            Notes = request.Notes
        };

        await _context.DailySalesSummaries.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(entity.Id, cancellationToken);
    }

    public async Task<Result<DailySalesSummaryDto>> UpdateAsync(Guid id, UpdateDailySalesSummaryRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _context.DailySalesSummaries.FindAsync([id], cancellationToken);
        if (entity is null || entity.IsDeleted) return Result<DailySalesSummaryDto>.Failure("Daily sales summary not found.");

        entity.VehicleLoadingId = request.VehicleLoadingId;
        entity.TruckId = request.TruckId;
        entity.DriverId = request.DriverId;
        entity.SalesmanId = request.SalesmanId;
        entity.TotalSales = request.TotalSales;
        entity.CashSales = request.CashSales;
        entity.CreditSales = request.CreditSales;
        entity.PackagesSold = request.PackagesSold;
        entity.RefillsSold = request.RefillsSold;
        entity.EmptyCylindersSold = request.EmptyCylindersSold;
        entity.AccessoriesSold = request.AccessoriesSold;
        entity.PaymentsCollected = request.PaymentsCollected;
        entity.DueCreated = request.DueCreated;
        entity.CylinderBalance = request.CylinderBalance;
        entity.OutstandingCylinders = request.OutstandingCylinders;
        entity.StockReturned = request.StockReturned;
        entity.Notes = request.Notes;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(entity.Id, cancellationToken);
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.DailySalesSummaries.FindAsync([id], cancellationToken);
        if (entity is null || entity.IsDeleted) return Result.Failure("Daily sales summary not found.");

        _context.DailySalesSummaries.Remove(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
