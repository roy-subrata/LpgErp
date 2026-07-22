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
    Task<Result<VehicleLoadingDto>> UpdateAsync(Guid id, UpdateVehicleLoadingRequest request, CancellationToken ct = default);
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
            Id = Guid.NewGuid(),
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

        // Validate warehouse availability per product (aggregated across lines) before mutating anything.
        var required = request.Items.GroupBy(i => i.ProductId).ToDictionary(g => g.Key, g => g.Sum(i => i.LoadedQuantity));
        var stockByProduct = new Dictionary<Guid, StockLevel>();
        foreach (var (productId, qty) in required)
        {
            var stock = await _context.StockLevels
                .FirstOrDefaultAsync(s => s.WarehouseId == request.WarehouseId && s.ProductId == productId, ct);
            if (stock is null || stock.Quantity < qty)
                return Result<VehicleLoadingDto>.Failure($"Insufficient warehouse stock for product {productId}.");
            stockByProduct[productId] = stock;
        }

        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            // Goods leave the warehouse onto the vehicle; total company stock (Product.CurrentStock) is unchanged.
            foreach (var (productId, qty) in required)
            {
                stockByProduct[productId].Quantity -= qty;
                await _context.StockMovements.AddAsync(new StockMovement
                {
                    ProductId = productId,
                    Type = StockMovementType.TransferOut,
                    Quantity = qty,
                    FromWarehouseId = request.WarehouseId,
                    Reference = LoadingReference(loading.Id),
                    MovementDate = DateTime.UtcNow
                }, ct);
            }

            await _context.VehicleLoadings.AddAsync(loading, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            throw;
        }

        return await GetByIdAsync(loading.Id, ct);
    }

    public async Task<Result<VehicleLoadingDto>> UpdateAsync(Guid id, UpdateVehicleLoadingRequest request, CancellationToken ct = default)
    {
        var loading = await _context.VehicleLoadings
            .Include(v => v.Items)
            .FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted, ct);
        if (loading is null) return Result<VehicleLoadingDto>.Failure("Vehicle loading not found.");
        if (loading.Status != VehicleLoadingStatus.Dispatched) return Result<VehicleLoadingDto>.Failure("Only dispatched vehicle loadings can be edited.");

        // Net stock effect of the edit. Positive delta = deduct from the warehouse, negative = return to it.
        // Same warehouse: per-product deltas; warehouse changed: return the full old load, deduct the full new one.
        var oldWarehouseId = loading.WarehouseId;
        var oldQty = loading.Items.GroupBy(i => i.ProductId).ToDictionary(g => g.Key, g => g.Sum(i => i.LoadedQuantity));
        var newQty = request.Items.GroupBy(i => i.ProductId).ToDictionary(g => g.Key, g => g.Sum(i => i.LoadedQuantity));

        var adjustments = new List<(Guid WarehouseId, Guid ProductId, int Delta)>();
        if (request.WarehouseId == oldWarehouseId)
        {
            foreach (var productId in oldQty.Keys.Union(newQty.Keys))
            {
                var delta = newQty.GetValueOrDefault(productId) - oldQty.GetValueOrDefault(productId);
                if (delta != 0) adjustments.Add((oldWarehouseId, productId, delta));
            }
        }
        else
        {
            foreach (var (productId, qty) in oldQty) adjustments.Add((oldWarehouseId, productId, -qty));
            foreach (var (productId, qty) in newQty) adjustments.Add((request.WarehouseId, productId, qty));
        }

        // Validate every deduction before mutating.
        var stocks = new Dictionary<(Guid WarehouseId, Guid ProductId), StockLevel>();
        foreach (var (warehouseId, productId, delta) in adjustments)
        {
            var stock = await _context.StockLevels.FirstOrDefaultAsync(s => s.WarehouseId == warehouseId && s.ProductId == productId, ct);
            if (delta > 0 && (stock is null || stock.Quantity < delta))
                return Result<VehicleLoadingDto>.Failure($"Insufficient warehouse stock for product {productId}.");
            if (stock is not null) stocks[(warehouseId, productId)] = stock;
        }

        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            foreach (var (warehouseId, productId, delta) in adjustments)
            {
                if (!stocks.TryGetValue((warehouseId, productId), out var stock))
                {
                    stock = new StockLevel { WarehouseId = warehouseId, ProductId = productId, Quantity = 0 };
                    await _context.StockLevels.AddAsync(stock, ct);
                    stocks[(warehouseId, productId)] = stock;
                }
                stock.Quantity -= delta;

                await _context.StockMovements.AddAsync(new StockMovement
                {
                    ProductId = productId,
                    Type = delta > 0 ? StockMovementType.TransferOut : StockMovementType.Return,
                    Quantity = Math.Abs(delta),
                    FromWarehouseId = delta > 0 ? warehouseId : null,
                    ToWarehouseId = delta > 0 ? null : warehouseId,
                    Reference = LoadingReference(loading.Id),
                    MovementDate = DateTime.UtcNow
                }, ct);
            }

            loading.LoadingDate = request.LoadingDate;
            loading.TruckId = request.TruckId;
            loading.DriverId = request.DriverId;
            loading.SalesmanId = request.SalesmanId;
            loading.WarehouseId = request.WarehouseId;
            loading.RouteId = request.RouteId;
            loading.Notes = request.Notes;

            _context.VehicleLoadingItems.RemoveRange(loading.Items);
            loading.Items = request.Items.Select(i => new VehicleLoadingItem
            {
                ProductId = i.ProductId,
                LoadedQuantity = i.LoadedQuantity
            }).ToList();

            await _unitOfWork.SaveChangesAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            throw;
        }

        return await GetByIdAsync(id, ct);
    }

    public async Task<Result<VehicleClosingDto>> CloseAsync(Guid loadingId, CreateVehicleClosingRequest request, CancellationToken ct = default)
    {
        var loading = await _context.VehicleLoadings
            .Include(v => v.Items)
            .FirstOrDefaultAsync(v => v.Id == loadingId && !v.IsDeleted, ct);
        if (loading is null) return Result<VehicleClosingDto>.Failure("Vehicle loading not found.");
        if (loading.Status != VehicleLoadingStatus.Dispatched)
            return Result<VehicleClosingDto>.Failure("This vehicle loading has already been closed.");

        if (request.Items.GroupBy(i => i.ProductId).Any(g => g.Count() > 1))
            return Result<VehicleClosingDto>.Failure("The closing contains duplicate lines for the same product.");

        // Reconcile the reported quantities against what was actually loaded.
        var loadedByProduct = loading.Items.GroupBy(i => i.ProductId).ToDictionary(g => g.Key, g => g.Sum(i => i.LoadedQuantity));
        foreach (var item in request.Items)
        {
            if (!loadedByProduct.TryGetValue(item.ProductId, out var loaded))
                return Result<VehicleClosingDto>.Failure($"Product {item.ProductId} was not on this loading.");
            if (item.SoldQuantity + item.ReturnedQuantity + item.DamagedQuantity > loaded)
                return Result<VehicleClosingDto>.Failure($"Sold + returned + damaged exceeds the loaded quantity for product {item.ProductId}.");
        }

        // Sales already recorded as delivered vehicle sales orders had their stock deducted at delivery.
        // The reported SoldQuantity is the day's TOTAL sold (orders + unrecorded cash sales), so it must
        // cover the recorded orders, and only the difference is deducted here — never both.
        var orderSoldByProduct = await _context.SalesOrderItems
            .Where(i => !i.IsDeleted && !i.SalesOrder.IsDeleted
                && i.SalesOrder.VehicleLoadingId == loadingId
                && i.SalesOrder.Status == SalesOrderStatus.Delivered)
            .GroupBy(i => i.ProductId)
            .Select(g => new { g.Key, Qty = g.Sum(x => x.Quantity) })
            .ToDictionaryAsync(x => x.Key, x => x.Qty, ct);

        foreach (var (productId, orderSold) in orderSoldByProduct)
        {
            var reportedSold = request.Items.Where(i => i.ProductId == productId).Sum(i => i.SoldQuantity);
            if (reportedSold < orderSold)
                return Result<VehicleClosingDto>.Failure(
                    $"Reported sold quantity ({reportedSold}) for product {productId} is less than the {orderSold} already sold via recorded vehicle sales orders.");
        }

        var closing = new VehicleClosing
        {
            Id = Guid.NewGuid(),
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
                LoadedQuantity = loadedByProduct.GetValueOrDefault(i.ProductId),
                SoldQuantity = i.SoldQuantity,
                ReturnedQuantity = i.ReturnedQuantity,
                DamagedQuantity = i.DamagedQuantity
            }).ToList()
        };

        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var reference = $"VC-{closing.Id.ToString()[..8].ToUpperInvariant()}";
            foreach (var item in request.Items)
            {
                // Unsold goods come back into the warehouse.
                if (item.ReturnedQuantity > 0)
                {
                    var stock = await _context.StockLevels
                        .FirstOrDefaultAsync(s => s.WarehouseId == loading.WarehouseId && s.ProductId == item.ProductId, ct);
                    if (stock is null)
                    {
                        stock = new StockLevel { WarehouseId = loading.WarehouseId, ProductId = item.ProductId, Quantity = 0 };
                        await _context.StockLevels.AddAsync(stock, ct);
                    }
                    stock.Quantity += item.ReturnedQuantity;

                    await _context.StockMovements.AddAsync(new StockMovement
                    {
                        ProductId = item.ProductId,
                        Type = StockMovementType.Return,
                        Quantity = item.ReturnedQuantity,
                        ToWarehouseId = loading.WarehouseId,
                        Reference = reference,
                        MovementDate = DateTime.UtcNow
                    }, ct);
                }

                // Sold and damaged goods permanently leave company inventory — but sales already
                // recorded as delivered vehicle sales orders were deducted at delivery, so only the
                // unrecorded remainder is deducted here.
                var unrecordedSold = item.SoldQuantity - orderSoldByProduct.GetValueOrDefault(item.ProductId);
                var gone = unrecordedSold + item.DamagedQuantity;
                if (gone > 0)
                {
                    var product = await _context.Products.FindAsync([item.ProductId], ct);
                    if (product is not null) product.CurrentStock -= gone;
                }
                if (unrecordedSold > 0)
                {
                    await _context.StockMovements.AddAsync(new StockMovement
                    {
                        ProductId = item.ProductId,
                        Type = StockMovementType.SaleOut,
                        Quantity = unrecordedSold,
                        FromWarehouseId = loading.WarehouseId,
                        Reference = reference,
                        MovementDate = DateTime.UtcNow
                    }, ct);
                }
                if (item.DamagedQuantity > 0)
                {
                    await _context.StockMovements.AddAsync(new StockMovement
                    {
                        ProductId = item.ProductId,
                        Type = StockMovementType.Adjustment,
                        Quantity = item.DamagedQuantity,
                        FromWarehouseId = loading.WarehouseId,
                        Reference = $"{reference} damaged",
                        MovementDate = DateTime.UtcNow
                    }, ct);
                }
            }

            loading.Status = VehicleLoadingStatus.Returned;

            await _context.VehicleClosings.AddAsync(closing, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            throw;
        }

        var result = await _context.VehicleClosings
            .Include(vc => vc.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(vc => vc.Id == closing.Id, ct);

        return Result<VehicleClosingDto>.Success(_mapper.Map<VehicleClosingDto>(result));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _context.VehicleLoadings
            .FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted, ct);
        if (entity is null) return Result.Failure("Vehicle loading not found.");
        if (entity.Status != VehicleLoadingStatus.Dispatched) return Result.Failure("Only dispatched vehicle loadings can be deleted.");

        // Read items untracked: removing the parent with tracked required children trips EF's severed-relationship check
        // before the soft-delete conversion in SaveChangesAsync runs.
        var items = await _context.VehicleLoadingItems.AsNoTracking()
            .Where(i => i.VehicleLoadingId == id && !i.IsDeleted)
            .ToListAsync(ct);

        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            // The loading never happened — put the goods back into the warehouse.
            foreach (var group in items.GroupBy(i => i.ProductId))
            {
                var qty = group.Sum(i => i.LoadedQuantity);
                var stock = await _context.StockLevels
                    .FirstOrDefaultAsync(s => s.WarehouseId == entity.WarehouseId && s.ProductId == group.Key, ct);
                if (stock is null)
                {
                    stock = new StockLevel { WarehouseId = entity.WarehouseId, ProductId = group.Key, Quantity = 0 };
                    await _context.StockLevels.AddAsync(stock, ct);
                }
                stock.Quantity += qty;

                await _context.StockMovements.AddAsync(new StockMovement
                {
                    ProductId = group.Key,
                    Type = StockMovementType.Return,
                    Quantity = qty,
                    ToWarehouseId = entity.WarehouseId,
                    Reference = $"{LoadingReference(entity.Id)} cancelled",
                    MovementDate = DateTime.UtcNow
                }, ct);
            }

            _context.VehicleLoadings.Remove(entity);
            await _unitOfWork.SaveChangesAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            throw;
        }

        return Result.Success();
    }

    private static string LoadingReference(Guid loadingId) => $"VL-{loadingId.ToString()[..8].ToUpperInvariant()}";
}
