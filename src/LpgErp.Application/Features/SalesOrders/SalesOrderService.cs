using AutoMapper;
using LpgErp.Application.Common.Interfaces;
using LpgErp.Application.Common.Models;
using LpgErp.Application.Features.SalesOrders.DTOs;
using LpgErp.Domain.Entities;
using LpgErp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LpgErp.Application.Features.SalesOrders;

public interface ISalesOrderService
{
    Task<Result<PagedResult<SalesOrderDto>>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<Result<SalesOrderDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<SalesOrderDto>> CreateAsync(CreateSalesOrderRequest request, CancellationToken cancellationToken = default);
    Task<Result<SalesOrderDto>> UpdateAsync(Guid id, UpdateSalesOrderRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<SalesOrderDto>> ConfirmAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<SalesOrderDto>> DeliverAsync(Guid id, CancellationToken cancellationToken = default);
}

public class SalesOrderService : ISalesOrderService
{
    private readonly IApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SalesOrderService(IApplicationDbContext context, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<SalesOrderDto>>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.SalesOrders
            .Where(so => !so.IsDeleted)
            .Include(so => so.Customer)
            .Include(so => so.Warehouse)
            .Include(so => so.Route)
            .OrderByDescending(so => so.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return Result<PagedResult<SalesOrderDto>>.Success(new PagedResult<SalesOrderDto>
        {
            Items = _mapper.Map<IReadOnlyList<SalesOrderDto>>(items),
            Pagination = new PaginationMeta { PageNumber = pageNumber, PageSize = pageSize, TotalCount = totalCount }
        });
    }

    public async Task<Result<SalesOrderDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.SalesOrders
            .Include(so => so.Customer)
            .Include(so => so.Warehouse)
            .Include(so => so.Route)
            .Include(so => so.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(so => so.Id == id && !so.IsDeleted, cancellationToken);

        if (entity is null) return Result<SalesOrderDto>.Failure("Sales order not found.");
        return Result<SalesOrderDto>.Success(_mapper.Map<SalesOrderDto>(entity));
    }

    public async Task<Result<SalesOrderDto>> CreateAsync(CreateSalesOrderRequest request, CancellationToken cancellationToken = default)
    {
        var order = new SalesOrder
        {
            OrderNumber = $"SO-{DateTime.UtcNow:yyyyMMddHHmmss}",
            CustomerId = request.CustomerId,
            WarehouseId = request.WarehouseId,
            Status = SalesOrderStatus.Draft,
            Discount = request.Discount,
            Notes = request.Notes,
            IsCreditSale = request.IsCreditSale,
            DueDate = request.DueDate,
            TransportCompanyId = request.TransportCompanyId,
            RouteId = request.RouteId,
            VehicleLoadingId = request.VehicleLoadingId,
            OrderDate = DateTime.UtcNow,
            Items = request.Items.Select(i => new SalesOrderItem
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                CylinderExchangeQuantity = i.CylinderExchangeQuantity,
                EmptyReturnedQuantity = i.EmptyReturnedQuantity
            }).ToList()
        };

        order.TotalAmount = order.Items.Sum(i => i.TotalPrice);

        await _context.SalesOrders.AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(order.Id, cancellationToken);
    }

    public async Task<Result<SalesOrderDto>> UpdateAsync(Guid id, UpdateSalesOrderRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _context.SalesOrders
            .Include(so => so.Items)
            .FirstOrDefaultAsync(so => so.Id == id && !so.IsDeleted, cancellationToken);

        if (entity is null) return Result<SalesOrderDto>.Failure("Sales order not found.");
        if (entity.Status != SalesOrderStatus.Draft)
            return Result<SalesOrderDto>.Failure("Only draft orders can be updated.");

        entity.CustomerId = request.CustomerId;
        entity.WarehouseId = request.WarehouseId;
        entity.Status = request.Status;
        entity.Discount = request.Discount;
        entity.Notes = request.Notes;
        entity.IsCreditSale = request.IsCreditSale;
        entity.DueDate = request.DueDate;
        entity.TransportCompanyId = request.TransportCompanyId;
        entity.RouteId = request.RouteId;
        entity.VehicleLoadingId = request.VehicleLoadingId;

        _context.SalesOrderItems.RemoveRange(entity.Items);
        entity.Items = request.Items.Select(i => new SalesOrderItem
        {
            SalesOrderId = id,
            ProductId = i.ProductId,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            CylinderExchangeQuantity = i.CylinderExchangeQuantity,
            EmptyReturnedQuantity = i.EmptyReturnedQuantity
        }).ToList();

        entity.TotalAmount = entity.Items.Sum(i => i.TotalPrice);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.SalesOrders.FindAsync([id], cancellationToken);
        if (entity is null) return Result.Failure("Sales order not found.");
        if (entity.Status != SalesOrderStatus.Draft)
            return Result.Failure("Only draft orders can be deleted.");

        _context.SalesOrders.Remove(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<SalesOrderDto>> ConfirmAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.SalesOrders.FindAsync([id], cancellationToken);
        if (entity is null) return Result<SalesOrderDto>.Failure("Sales order not found.");
        if (entity.Status != SalesOrderStatus.Draft)
            return Result<SalesOrderDto>.Failure("Only draft orders can be confirmed.");

        entity.Status = SalesOrderStatus.Confirmed;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<Result<SalesOrderDto>> DeliverAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.SalesOrders
            .Include(so => so.Items)
            .FirstOrDefaultAsync(so => so.Id == id && !so.IsDeleted, cancellationToken);

        if (entity is null) return Result<SalesOrderDto>.Failure("Sales order not found.");
        if (entity.Status != SalesOrderStatus.Confirmed)
            return Result<SalesOrderDto>.Failure("Only confirmed orders can be delivered.");

        if (entity.VehicleLoadingId is Guid loadingId)
        {
            // Mobile sale: draw down the vehicle's loaded stock. Warehouse stock was already
            // deducted when the vehicle was loaded, so it is NOT touched here — this is what
            // prevents the same sale from being deducted twice.
            var loading = await _context.VehicleLoadings
                .Include(v => v.Items)
                .FirstOrDefaultAsync(v => v.Id == loadingId && !v.IsDeleted, cancellationToken);
            if (loading is null) return Result<SalesOrderDto>.Failure("Vehicle loading not found.");
            if (loading.Status != VehicleLoadingStatus.Dispatched)
                return Result<SalesOrderDto>.Failure("The vehicle for this order has already been closed.");

            var loadedByProduct = loading.Items.GroupBy(i => i.ProductId).ToDictionary(g => g.Key, g => g.Sum(i => i.LoadedQuantity));
            var soldByProduct = await _context.SalesOrderItems
                .Where(i => !i.IsDeleted && !i.SalesOrder.IsDeleted
                    && i.SalesOrder.VehicleLoadingId == loadingId
                    && i.SalesOrder.Status == SalesOrderStatus.Delivered
                    && i.SalesOrderId != entity.Id)
                .GroupBy(i => i.ProductId)
                .Select(g => new { g.Key, Qty = g.Sum(x => x.Quantity) })
                .ToDictionaryAsync(x => x.Key, x => x.Qty, cancellationToken);

            // Aggregate order lines per product so duplicate lines can't jointly exceed the vehicle stock.
            foreach (var group in entity.Items.GroupBy(i => i.ProductId))
            {
                var remaining = loadedByProduct.GetValueOrDefault(group.Key) - soldByProduct.GetValueOrDefault(group.Key);
                if (group.Sum(i => i.Quantity) > remaining)
                    return Result<SalesOrderDto>.Failure($"Insufficient stock on the vehicle for product {group.Key} (remaining {remaining}).");
            }

            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                foreach (var item in entity.Items)
                {
                    var product = await _context.Products.FindAsync([item.ProductId], cancellationToken);
                    if (product is not null) product.CurrentStock -= item.Quantity;

                    // Live-update the loading's sold counter so the vehicle card shows real progress.
                    var loadingItem = loading.Items.FirstOrDefault(l => l.ProductId == item.ProductId);
                    if (loadingItem is not null) loadingItem.SoldQuantity += item.Quantity;

                    await _context.StockMovements.AddAsync(new StockMovement
                    {
                        ProductId = item.ProductId,
                        Type = StockMovementType.SaleOut,
                        Quantity = item.Quantity,
                        FromWarehouseId = loading.WarehouseId,
                        SalesOrderId = entity.Id,
                        Reference = entity.OrderNumber,
                        MovementDate = DateTime.UtcNow
                    }, cancellationToken);
                }

                await ApplyCylinderLedgerAsync(entity, cancellationToken);

                entity.Status = SalesOrderStatus.Delivered;
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }

            return await GetByIdAsync(id, cancellationToken);
        }

        // Direct warehouse sale: validate availability first (aggregated per product), then deduct warehouse stock.
        var requiredByProduct = entity.Items.GroupBy(i => i.ProductId).ToDictionary(g => g.Key, g => g.Sum(i => i.Quantity));
        var stockByProduct = new Dictionary<Guid, StockLevel>();
        foreach (var (productId, qty) in requiredByProduct)
        {
            var stockLevel = await _context.StockLevels
                .FirstOrDefaultAsync(s => s.WarehouseId == entity.WarehouseId && s.ProductId == productId, cancellationToken);

            if (stockLevel is null || stockLevel.Quantity < qty)
                return Result<SalesOrderDto>.Failure($"Insufficient stock for product {productId} in warehouse.");
            stockByProduct[productId] = stockLevel;
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            foreach (var (productId, qty) in requiredByProduct)
            {
                stockByProduct[productId].Quantity -= qty;

                var product = await _context.Products.FindAsync([productId], cancellationToken);
                if (product is not null) product.CurrentStock -= qty;

                await _context.StockMovements.AddAsync(new StockMovement
                {
                    ProductId = productId,
                    Type = StockMovementType.SaleOut,
                    Quantity = qty,
                    FromWarehouseId = entity.WarehouseId,
                    SalesOrderId = entity.Id,
                    Reference = entity.OrderNumber,
                    MovementDate = DateTime.UtcNow
                }, cancellationToken);
            }

            await ApplyCylinderLedgerAsync(entity, cancellationToken);

            entity.Status = SalesOrderStatus.Delivered;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }

        return await GetByIdAsync(id, cancellationToken);
    }

    /// <summary>
    /// Records the cylinder movement of a delivered sale on the customer's cylinder ledger.
    /// Gas-refill lines only: the customer receives filled cylinders and hands back empties.
    /// EmptyReturnedQuantity null = full swap; 0 = advance refill (cylinder owed, tracked as outstanding).
    /// Packages/empty-cylinder sales transfer ownership and do not touch the ledger.
    /// </summary>
    private async Task ApplyCylinderLedgerAsync(SalesOrder order, CancellationToken cancellationToken)
    {
        foreach (var item in order.Items)
        {
            var product = await _context.Products.FindAsync([item.ProductId], cancellationToken);
            if (product is null || product.Type != ProductType.GasRefill) continue;
            if (product.BrandId is null || product.CylinderSizeId is null) continue;

            var balance = await _context.CustomerCylinderBalances.FirstOrDefaultAsync(b =>
                b.CustomerId == order.CustomerId
                && b.BrandId == product.BrandId
                && b.CylinderSizeId == product.CylinderSizeId
                && !b.IsDeleted, cancellationToken);

            if (balance is null)
            {
                balance = new CustomerCylinderBalance
                {
                    CustomerId = order.CustomerId,
                    BrandId = product.BrandId.Value,
                    CylinderSizeId = product.CylinderSizeId.Value
                };
                await _context.CustomerCylinderBalances.AddAsync(balance, cancellationToken);
            }

            balance.Received += item.Quantity;
            balance.Returned += item.EmptyReturnedQuantity ?? item.Quantity;
        }
    }
}
