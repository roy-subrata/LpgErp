using AutoMapper;
using LpgErp.Application.Common.Interfaces;
using LpgErp.Application.Common.Models;
using LpgErp.Application.Features.Payments;
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
    private readonly INotificationService _notificationService;

    public SalesOrderService(IApplicationDbContext context, IUnitOfWork unitOfWork, IMapper mapper, INotificationService notificationService)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _notificationService = notificationService;
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

    /// <summary>
    /// A discount may never exceed the order's goods value — an order whose NetAmount is negative would
    /// read as money owed back to the customer and would corrupt revenue and receivable figures.
    /// </summary>
    private static string? ValidateDiscount(decimal discount, decimal totalAmount)
    {
        if (discount < 0) return "Discount cannot be negative.";
        if (discount > totalAmount) return $"Discount ({discount:N2}) cannot exceed the order total ({totalAmount:N2}).";
        return null;
    }

    public async Task<Result<SalesOrderDto>> CreateAsync(CreateSalesOrderRequest request, CancellationToken cancellationToken = default)
    {
        var order = new SalesOrder
        {
            OrderNumber = $"SO-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..4]}",
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
                EmptyReturnedQuantity = i.EmptyReturnedQuantity,
                DamagedReturnedQuantity = i.DamagedReturnedQuantity
            }).ToList()
        };

        order.TotalAmount = order.Items.Sum(i => i.TotalPrice);

        if (ValidateDiscount(order.Discount, order.TotalAmount) is string discountError)
            return Result<SalesOrderDto>.Failure(discountError);

        // Money collected at the counter is recorded with the order itself, in the same save, so a
        // cash or bKash sale can never end up with no record of how it was paid.
        if (request.Payment is { Amount: > 0 } paymentRequest)
        {
            var netAmount = order.TotalAmount - order.Discount;
            if (paymentRequest.Amount > netAmount)
                return Result<SalesOrderDto>.Failure($"Payment ({paymentRequest.Amount:N2}) exceeds the order total ({netAmount:N2}).");

            if (await PaymentAccountRules.ValidateAsync(_context, paymentRequest.PaymentAccountId, paymentRequest.Method, cancellationToken) is string accountError)
                return Result<SalesOrderDto>.Failure(accountError);

            order.Payments.Add(new Payment
            {
                Method = paymentRequest.Method,
                PaymentAccountId = paymentRequest.PaymentAccountId,
                Direction = PaymentDirection.Inbound,
                Amount = paymentRequest.Amount,
                PaymentDate = order.OrderDate,
                Reference = paymentRequest.Reference,
                Notes = "Collected at point of sale."
            });
        }

        await _context.SalesOrders.AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            var customer = await _context.Customers.FindAsync([request.CustomerId], cancellationToken);
            await _notificationService.NotifySaleCreatedAsync(
                order.OrderNumber,
                customer?.Name ?? "Unknown",
                order.TotalAmount - order.Discount,
                request.CustomerId);
        }
        catch { /* notification failure should not break the main operation */ }

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

        // Check the discount against the incoming items before mutating the tracked entity.
        if (ValidateDiscount(request.Discount, request.Items.Sum(i => i.Quantity * i.UnitPrice)) is string discountError)
            return Result<SalesOrderDto>.Failure(discountError);

        entity.CustomerId = request.CustomerId;
        entity.WarehouseId = request.WarehouseId;
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
            EmptyReturnedQuantity = i.EmptyReturnedQuantity,
            DamagedReturnedQuantity = i.DamagedReturnedQuantity
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
                    if (product is not null)
                    {
                        product.CurrentStock = Math.Max(0, product.CurrentStock - item.Quantity);
                        if (product.CurrentStock <= product.MinimumStock)
                        {
                            try
                            {
                                await _notificationService.NotifyStockLowAsync(product.Name, product.CurrentStock, product.MinimumStock);
                            }
                            catch { /* notification failure should not break the main operation */ }
                        }
                    }

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

        // Direct warehouse sale: validate availability inside the transaction to prevent TOCTOU races.
        var requiredByProduct = entity.Items.GroupBy(i => i.ProductId).ToDictionary(g => g.Key, g => g.Sum(i => i.Quantity));

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var stockByProduct = new Dictionary<Guid, StockLevel>();
            foreach (var (productId, qty) in requiredByProduct)
            {
                var stockLevel = await _context.StockLevels
                    .FirstOrDefaultAsync(s => s.WarehouseId == entity.WarehouseId && s.ProductId == productId, cancellationToken);

                if (stockLevel is null || stockLevel.Quantity < qty)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result<SalesOrderDto>.Failure($"Insufficient stock for product {productId} in warehouse.");
                }
                stockByProduct[productId] = stockLevel;
            }

            foreach (var (productId, qty) in requiredByProduct)
            {
                stockByProduct[productId].Quantity -= qty;

                var product = await _context.Products.FindAsync([productId], cancellationToken);
                if (product is not null)
                {
                    product.CurrentStock = Math.Max(0, product.CurrentStock - qty);
                    if (product.CurrentStock <= product.MinimumStock)
                    {
                        try
                        {
                            await _notificationService.NotifyStockLowAsync(product.Name, product.CurrentStock, product.MinimumStock);
                        }
                        catch { /* notification failure should not break the main operation */ }
                    }
                }

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
    /// Records the cylinder movement of a delivered sale: the customer's cylinder ledger, and the
    /// empties they hand back arriving in the warehouse.
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

            await ReceiveReturnedEmptiesAsync(order, item, product, cancellationToken);

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

    /// <summary>
    /// Puts the empties handed back on a refill into warehouse stock. Leaking ones are counted
    /// separately, since they cannot be sold or sent for refilling until the company takes them.
    /// </summary>
    private async Task ReceiveReturnedEmptiesAsync(SalesOrder order, SalesOrderItem item, Product refill, CancellationToken cancellationToken)
    {
        var returned = item.EmptyReturnedQuantity ?? item.Quantity;
        if (returned <= 0) return;

        var emptyProduct = await _context.Products.FirstOrDefaultAsync(p => !p.IsDeleted
            && p.Type == ProductType.EmptyCylinder
            && p.BrandId == refill.BrandId
            && p.CylinderSizeId == refill.CylinderSizeId, cancellationToken);

        // Without a matching empty-cylinder product there is nowhere to put them; the customer's
        // cylinder ledger still records the return.
        if (emptyProduct is null) return;

        var damaged = Math.Min(item.DamagedReturnedQuantity, returned);
        var good = returned - damaged;

        var stock = await _context.StockLevels
            .FirstOrDefaultAsync(s => s.WarehouseId == order.WarehouseId && s.ProductId == emptyProduct.Id, cancellationToken);

        if (stock is null)
        {
            stock = new StockLevel { WarehouseId = order.WarehouseId, ProductId = emptyProduct.Id, Quantity = 0 };
            await _context.StockLevels.AddAsync(stock, cancellationToken);
        }

        stock.Quantity += good;
        stock.DamagedQuantity += damaged;

        var stored = await _context.Products.FindAsync([emptyProduct.Id], cancellationToken);
        if (stored is not null)
        {
            stored.CurrentStock += good;
            stored.DamagedStock += damaged;
        }

        await _context.StockMovements.AddAsync(new StockMovement
        {
            ProductId = emptyProduct.Id,
            Type = StockMovementType.Return,
            Quantity = returned,
            ToWarehouseId = order.WarehouseId,
            SalesOrderId = order.Id,
            Reference = damaged > 0
                ? $"{order.OrderNumber} · empties returned ({damaged} leaking)"
                : $"{order.OrderNumber} · empties returned",
            MovementDate = DateTime.UtcNow
        }, cancellationToken);
    }
}
