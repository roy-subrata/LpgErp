using AutoMapper;
using LpgErp.Application.Common.Interfaces;
using LpgErp.Application.Common.Models;
using LpgErp.Application.Features.Payments;
using LpgErp.Application.Features.PurchaseOrders.DTOs;
using LpgErp.Domain.Entities;
using LpgErp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LpgErp.Application.Features.PurchaseOrders;

public interface IPurchaseOrderService
{
    Task<Result<PagedResult<PurchaseOrderDto>>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<Result<PurchaseOrderDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<PurchaseOrderDto>> CreateAsync(CreatePurchaseOrderRequest request, CancellationToken cancellationToken = default);
    Task<Result<PurchaseOrderDto>> UpdateAsync(Guid id, UpdatePurchaseOrderRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<PurchaseOrderDto>> ConfirmAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<PurchaseOrderDto>> ReceiveAsync(Guid id, ReceivePurchaseOrderRequest request, CancellationToken cancellationToken = default);
}

public class PurchaseOrderService : IPurchaseOrderService
{
    private readonly IApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public PurchaseOrderService(IApplicationDbContext context, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<PurchaseOrderDto>>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.PurchaseOrders
            .Where(po => !po.IsDeleted)
            .Include(po => po.Supplier)
            .Include(po => po.Warehouse)
            .OrderByDescending(po => po.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return Result<PagedResult<PurchaseOrderDto>>.Success(new PagedResult<PurchaseOrderDto>
        {
            Items = _mapper.Map<IReadOnlyList<PurchaseOrderDto>>(items),
            Pagination = new PaginationMeta { PageNumber = pageNumber, PageSize = pageSize, TotalCount = totalCount }
        });
    }

    public async Task<Result<PurchaseOrderDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.PurchaseOrders
            .Include(po => po.Supplier)
            .Include(po => po.Warehouse)
            .Include(po => po.Items).ThenInclude(i => i.Product)
            .Include(po => po.Leakages).ThenInclude(l => l.Brand)
            .Include(po => po.Leakages).ThenInclude(l => l.CylinderSize)
            .FirstOrDefaultAsync(po => po.Id == id && !po.IsDeleted, cancellationToken);

        if (entity is null) return Result<PurchaseOrderDto>.Failure("Purchase order not found.");
        return Result<PurchaseOrderDto>.Success(_mapper.Map<PurchaseOrderDto>(entity));
    }

    public async Task<Result<PurchaseOrderDto>> CreateAsync(CreatePurchaseOrderRequest request, CancellationToken cancellationToken = default)
    {
        var order = new PurchaseOrder
        {
            OrderNumber = $"PO-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..4]}",
            SupplierId = request.SupplierId,
            WarehouseId = request.WarehouseId,
            Status = PurchaseOrderStatus.Draft,
            ExpectedDeliveryDate = request.ExpectedDeliveryDate,
            DueDate = request.DueDate,
            TransportCompanyId = request.TransportCompanyId,
            TransportationCost = request.TransportationCost,
            Notes = request.Notes,
            OrderDate = DateTime.UtcNow,
            Items = request.Items.Select(i => new PurchaseOrderItem
            {
                ProductId = i.ProductId,
                OrderedQuantity = i.OrderedQuantity,
                UnitPrice = i.UnitPrice,
                EmptyReturnedQuantity = i.EmptyReturnedQuantity
            }).ToList()
        };

        order.TotalAmount = order.Items.Sum(i => i.TotalPrice);

        if (await BuildLeakagesAsync(order, request.Leakages, cancellationToken) is string leakageError)
            return Result<PurchaseOrderDto>.Failure(leakageError);

        // Transactional from here: the supplier's commission balance is read, validated against, and
        // decremented in one request, and two concurrent orders against the same supplier must not
        // both pass validation against the same starting balance (the same TOCTOU class fixed
        // elsewhere for the cylinder ledger — see CustomerCylinderLedgerService.AdjustBalanceAsync).
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var supplier = await _context.Suppliers.FirstOrDefaultAsync(s => s.Id == request.SupplierId && !s.IsDeleted, cancellationToken);
            if (supplier is null)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result<PurchaseOrderDto>.Failure("Supplier not found.");
            }
            if (ApplyCommission(order, supplier, request.CommissionCreditApplied) is string commissionError)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result<PurchaseOrderDto>.Failure(commissionError);
            }

            // Anything paid to the supplier up front is recorded with the order in the same save, so
            // the outbound side carries the same "how was this paid" detail as the customer side.
            if (request.Payment is { Amount: > 0 } paymentRequest)
            {
                var payable = order.NetPayable;
                if (paymentRequest.Amount > payable)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result<PurchaseOrderDto>.Failure($"Payment ({paymentRequest.Amount:N2}) exceeds the amount payable ({payable:N2}).");
                }

                if (await PaymentAccountRules.ValidateAsync(_context, paymentRequest.PaymentAccountId, paymentRequest.Method, cancellationToken) is string accountError)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result<PurchaseOrderDto>.Failure(accountError);
                }

                order.Payments.Add(new Payment
                {
                    Method = paymentRequest.Method,
                    PaymentAccountId = paymentRequest.PaymentAccountId,
                    Direction = PaymentDirection.Outbound,
                    Amount = paymentRequest.Amount,
                    PaymentDate = order.OrderDate ?? DateTime.UtcNow,
                    Reference = paymentRequest.Reference,
                    Notes = "Paid at order entry."
                });
            }

            await _context.PurchaseOrders.AddAsync(order, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }

        return await GetByIdAsync(order.Id, cancellationToken);
    }

    public async Task<Result<PurchaseOrderDto>> UpdateAsync(Guid id, UpdatePurchaseOrderRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _context.PurchaseOrders
            .Include(po => po.Items)
            .Include(po => po.Leakages)
            .FirstOrDefaultAsync(po => po.Id == id && !po.IsDeleted, cancellationToken);

        if (entity is null) return Result<PurchaseOrderDto>.Failure("Purchase order not found.");
        if (entity.Status != PurchaseOrderStatus.Draft)
            return Result<PurchaseOrderDto>.Failure("Only draft orders can be updated.");

        entity.SupplierId = request.SupplierId;
        entity.WarehouseId = request.WarehouseId;
        entity.ExpectedDeliveryDate = request.ExpectedDeliveryDate;
        entity.ReceivedDate = request.ReceivedDate;
        entity.DueDate = request.DueDate;
        entity.TransportCompanyId = request.TransportCompanyId;
        entity.TransportationCost = request.TransportationCost;
        entity.Notes = request.Notes;

        _context.PurchaseOrderItems.RemoveRange(entity.Items);
        entity.Items = request.Items.Select(i => new PurchaseOrderItem
        {
            PurchaseOrderId = id,
            ProductId = i.ProductId,
            OrderedQuantity = i.OrderedQuantity,
            UnitPrice = i.UnitPrice,
            EmptyReturnedQuantity = i.EmptyReturnedQuantity
        }).ToList();

        entity.TotalAmount = entity.Items.Sum(i => i.TotalPrice);

        _context.PurchaseOrderLeakages.RemoveRange(entity.Leakages);
        if (await BuildLeakagesAsync(entity, request.Leakages, cancellationToken) is string leakageError)
            return Result<PurchaseOrderDto>.Failure(leakageError);

        // Transactional from here: refunding the old commission and applying the new amount must be
        // read-checked-written against a supplier balance that cannot shift underneath a concurrent
        // request — same TOCTOU class as CreateAsync above.
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // Refund any commission previously applied to the original supplier before the order is re-priced/re-assigned.
            if (entity.CommissionApplied > 0)
            {
                var originalSupplier = await _context.Suppliers.FindAsync([entity.SupplierId], cancellationToken);
                if (originalSupplier is not null) originalSupplier.CommissionBalance += entity.CommissionApplied;
                entity.CommissionApplied = 0;
            }

            var supplier = await _context.Suppliers.FirstOrDefaultAsync(s => s.Id == request.SupplierId && !s.IsDeleted, cancellationToken);
            if (supplier is null)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result<PurchaseOrderDto>.Failure("Supplier not found.");
            }
            if (ApplyCommission(entity, supplier, request.CommissionCreditApplied) is string commissionError)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result<PurchaseOrderDto>.Failure(commissionError);
            }

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

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.PurchaseOrders.FindAsync([id], cancellationToken);
        if (entity is null) return Result.Failure("Purchase order not found.");
        if (entity.Status != PurchaseOrderStatus.Draft)
            return Result.Failure("Only draft orders can be deleted.");

        // Return any applied commission to the supplier's balance since the order is being removed.
        if (entity.CommissionApplied > 0)
        {
            var supplier = await _context.Suppliers.FindAsync([entity.SupplierId], cancellationToken);
            if (supplier is not null) supplier.CommissionBalance += entity.CommissionApplied;
            entity.CommissionApplied = 0;
        }

        _context.PurchaseOrders.Remove(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<PurchaseOrderDto>> ConfirmAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.PurchaseOrders.FindAsync([id], cancellationToken);
        if (entity is null) return Result<PurchaseOrderDto>.Failure("Purchase order not found.");
        if (entity.Status != PurchaseOrderStatus.Draft)
            return Result<PurchaseOrderDto>.Failure("Only draft orders can be confirmed.");

        entity.Status = PurchaseOrderStatus.Confirmed;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    /// <summary>
    /// Validates and attaches the leaking cylinders going back with an order, and totals the credit
    /// the company is giving for them. Returns an error message, or null when the lines are sound.
    /// </summary>
    private async Task<string?> BuildLeakagesAsync(PurchaseOrder order, List<CreateLeakageRequest> requests, CancellationToken ct)
    {
        order.Leakages.Clear();
        order.LeakageCredit = 0m;

        foreach (var leak in requests.Where(l => l.Quantity > 0))
        {
            var product = await FindEmptyCylinderProductAsync(leak.BrandId, leak.CylinderSizeId, ct);
            if (product is null)
                return "No empty-cylinder product exists for the leaking brand and size.";

            // You can only send back leaking cylinders you actually hold.
            var damagedHeld = await _context.StockLevels
                .Where(s => s.WarehouseId == order.WarehouseId && s.ProductId == product.Id)
                .Select(s => (int?)s.DamagedQuantity)
                .FirstOrDefaultAsync(ct) ?? 0;

            if (leak.Quantity > damagedHeld)
                return $"Only {damagedHeld} damaged {product.Name} in this warehouse, cannot return {leak.Quantity}.";

            if (leak.Resolution == LeakageResolution.CreditAdjustment && leak.CreditAmount <= 0)
                return "A credit adjustment needs the amount the company is taking off the bill.";

            order.Leakages.Add(new PurchaseOrderLeakage
            {
                BrandId = leak.BrandId,
                CylinderSizeId = leak.CylinderSizeId,
                Quantity = leak.Quantity,
                Resolution = leak.Resolution,
                // Only a credit settles in money; free refills and replacements settle in goods.
                CreditAmount = leak.Resolution == LeakageResolution.CreditAdjustment ? leak.CreditAmount : 0m,
                Notes = leak.Notes,
            });
        }

        order.LeakageCredit = order.Leakages.Sum(l => l.CreditAmount);

        if (order.LeakageCredit > order.TotalAmount + order.TransportationCost)
            return "The leakage credit is more than the order is worth.";

        return null;
    }

    /// <summary>
    /// Hands the leaking cylinders over to the company and brings back whatever was agreed:
    /// a free refill, a replacement empty, or nothing at all when they settled it in money.
    /// </summary>
    private async Task<string?> SettleLeakagesAsync(PurchaseOrder order, List<SettleLeakageRequest> settlements, CancellationToken ct)
    {
        foreach (var settlement in settlements.Where(s => s.SettledQuantity > 0))
        {
            var leak = order.Leakages.FirstOrDefault(l => l.Id == settlement.LeakageId);
            if (leak is null) return "Leakage line not found on this order.";

            var remaining = leak.Quantity - leak.SettledQuantity;
            if (settlement.SettledQuantity > remaining)
                return $"Only {remaining} leaking cylinder(s) left to settle on that line.";

            var emptyProduct = await FindEmptyCylinderProductAsync(leak.BrandId, leak.CylinderSizeId, ct);
            if (emptyProduct is null) return "No empty-cylinder product exists for the leaking brand and size.";

            var stock = await GetOrCreateStockAsync(order.WarehouseId, emptyProduct.Id, ct);

            if (stock.DamagedQuantity < settlement.SettledQuantity)
                return $"Only {stock.DamagedQuantity} damaged {emptyProduct.Name} in stock to hand over.";

            // The faulty cylinders physically leave, whatever the company gives back.
            stock.DamagedQuantity -= settlement.SettledQuantity;
            var product = await _context.Products.FindAsync([emptyProduct.Id], ct);
            if (product is not null) product.DamagedStock -= settlement.SettledQuantity;

            leak.SettledQuantity += settlement.SettledQuantity;

            await _context.StockMovements.AddAsync(new StockMovement
            {
                ProductId = emptyProduct.Id,
                Type = StockMovementType.PurchaseReturnOut,
                Quantity = settlement.SettledQuantity,
                FromWarehouseId = order.WarehouseId,
                PurchaseOrderId = order.Id,
                Reference = $"{order.OrderNumber} · leakage to company",
                MovementDate = DateTime.UtcNow
            }, ct);

            // What comes back depends on how the company settled it.
            var (incomingProductId, incomingLabel) = leak.Resolution switch
            {
                // A free refill returns a full cylinder — the refill product for that brand and size.
                LeakageResolution.FreeRefill => (
                    (await FindRefillProductAsync(leak.BrandId, leak.CylinderSizeId, ct))?.Id,
                    "free refill"),

                // A replacement returns a good empty of the same kind.
                LeakageResolution.Replacement => ((Guid?)emptyProduct.Id, "replacement"),

                // A credit returns money, already applied to the order's payable — no goods.
                _ => (null, "credit"),
            };

            if (leak.Resolution == LeakageResolution.FreeRefill && incomingProductId is null)
                return "No gas-refill product exists for the leaking brand and size, so a free refill cannot be recorded.";

            if (incomingProductId is Guid inId)
            {
                var inStock = await GetOrCreateStockAsync(order.WarehouseId, inId, ct);
                inStock.Quantity += settlement.SettledQuantity;

                var inProduct = await _context.Products.FindAsync([inId], ct);
                if (inProduct is not null) inProduct.CurrentStock += settlement.SettledQuantity;

                await _context.StockMovements.AddAsync(new StockMovement
                {
                    ProductId = inId,
                    Type = StockMovementType.PurchaseIn,
                    Quantity = settlement.SettledQuantity,
                    ToWarehouseId = order.WarehouseId,
                    PurchaseOrderId = order.Id,
                    Reference = $"{order.OrderNumber} · leakage {incomingLabel}",
                    MovementDate = DateTime.UtcNow
                }, ct);
            }
        }

        return null;
    }

    /// <summary>
    /// The stock row for a product at a warehouse, creating it if needed.
    /// Checks the tracked entities first: one receive can touch the same product twice (goods in,
    /// then a leakage replacement), and a query does not see a row added moments earlier — which
    /// silently produced two stock rows for the same product.
    /// </summary>
    private async Task<StockLevel> GetOrCreateStockAsync(Guid warehouseId, Guid productId, CancellationToken ct)
    {
        var tracked = _context.StockLevels.Local
            .FirstOrDefault(s => s.WarehouseId == warehouseId && s.ProductId == productId);
        if (tracked is not null) return tracked;

        var existing = await _context.StockLevels
            .FirstOrDefaultAsync(s => s.WarehouseId == warehouseId && s.ProductId == productId, ct);
        if (existing is not null) return existing;

        var created = new StockLevel { WarehouseId = warehouseId, ProductId = productId, Quantity = 0 };
        await _context.StockLevels.AddAsync(created, ct);
        return created;
    }

    private Task<Product?> FindRefillProductAsync(Guid brandId, Guid sizeId, CancellationToken ct) =>
        _context.Products.FirstOrDefaultAsync(p => !p.IsDeleted
            && p.Type == ProductType.GasRefill
            && p.BrandId == brandId
            && p.CylinderSizeId == sizeId, ct);

    private Task<Product?> FindEmptyCylinderProductAsync(Guid brandId, Guid sizeId, CancellationToken ct) =>
        _context.Products.FirstOrDefaultAsync(p => !p.IsDeleted
            && p.Type == ProductType.EmptyCylinder
            && p.BrandId == brandId
            && p.CylinderSizeId == sizeId, ct);

    public async Task<Result<PurchaseOrderDto>> ReceiveAsync(Guid id, ReceivePurchaseOrderRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _context.PurchaseOrders
            .Include(po => po.Items).ThenInclude(i => i.Product)
            .Include(po => po.Leakages)
            .FirstOrDefaultAsync(po => po.Id == id && !po.IsDeleted, cancellationToken);

        if (entity is null) return Result<PurchaseOrderDto>.Failure("Purchase order not found.");
        if (entity.Status != PurchaseOrderStatus.Confirmed && entity.Status != PurchaseOrderStatus.InTransit && entity.Status != PurchaseOrderStatus.PartiallyReceived)
            return Result<PurchaseOrderDto>.Failure("Order cannot be received in current status.");

        var supplier = await _context.Suppliers.FindAsync([entity.SupplierId], cancellationToken);
        decimal commissionAccrued = 0m;

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
        foreach (var receiveItem in request.Items)
        {
            var orderItem = entity.Items.FirstOrDefault(i => i.ProductId == receiveItem.ProductId);
            if (orderItem is not null)
            {
                if (receiveItem.ReceivedQuantity < 0 || receiveItem.DamagedQuantity < 0 || receiveItem.MissingQuantity < 0)
                    return Result<PurchaseOrderDto>.Failure("Quantities cannot be negative.");

                var remainingToReceive = orderItem.OrderedQuantity - orderItem.ReceivedQuantity;
                if (receiveItem.ReceivedQuantity > remainingToReceive)
                    return Result<PurchaseOrderDto>.Failure($"Cannot receive {receiveItem.ReceivedQuantity} of product {orderItem.ProductId}. Only {remainingToReceive} remaining to receive.");

                if (receiveItem.DamagedQuantity > receiveItem.ReceivedQuantity)
                    return Result<PurchaseOrderDto>.Failure("Damaged quantity cannot exceed received quantity.");

                orderItem.ReceivedQuantity += receiveItem.ReceivedQuantity;
                orderItem.DamagedQuantity += receiveItem.DamagedQuantity;
                orderItem.MissingQuantity += receiveItem.MissingQuantity;

                var goodQuantity = receiveItem.ReceivedQuantity - receiveItem.DamagedQuantity;
                if (goodQuantity > 0)
                {
                    var stockLevel = await GetOrCreateStockAsync(entity.WarehouseId, orderItem.ProductId, cancellationToken);
                    stockLevel.Quantity += goodQuantity;

                    var product = await _context.Products.FindAsync([orderItem.ProductId], cancellationToken);
                    if (product is not null)
                    {
                        product.CurrentStock += goodQuantity;

                        // Commission is granted per physical cylinder received (empty cylinders and new packages carry a cylinder).
                        if (supplier is not null && supplier.CommissionPerCylinder > 0
                            && (product.Type == ProductType.EmptyCylinder || product.Type == ProductType.NewPackage))
                        {
                            commissionAccrued += goodQuantity * supplier.CommissionPerCylinder;
                        }
                    }

                    await _context.StockMovements.AddAsync(new StockMovement
                    {
                        ProductId = orderItem.ProductId,
                        Type = StockMovementType.PurchaseIn,
                        Quantity = goodQuantity,
                        ToWarehouseId = entity.WarehouseId,
                        PurchaseOrderId = entity.Id,
                        Reference = entity.OrderNumber,
                        MovementDate = DateTime.UtcNow
                    }, cancellationToken);
                }

                // The company refills one cylinder for one empty, so empties leave the warehouse as
                // refills arrive. Without this the empty stock only ever grew.
                if (orderItem.Product.Type == ProductType.GasRefill)
                {
                    if (receiveItem.EmptySentQuantity is < 0)
                        return Result<PurchaseOrderDto>.Failure("Empties sent cannot be negative.");

                    var emptyProduct = orderItem.Product.BrandId is Guid brandId && orderItem.Product.CylinderSizeId is Guid sizeId
                        ? await FindEmptyCylinderProductAsync(brandId, sizeId, cancellationToken)
                        : null;

                    var emptyStock = emptyProduct is null
                        ? null
                        : await GetOrCreateStockAsync(entity.WarehouseId, emptyProduct.Id, cancellationToken);

                    var available = emptyStock?.Quantity ?? 0;

                    // An explicit figure is a claim about what physically went back, so it is checked.
                    // Left blank, we send what we have and let the rest stand as owed — the goods have
                    // arrived either way, and owing the company cylinders is a normal state.
                    int emptiesToSend;
                    if (receiveItem.EmptySentQuantity is int stated)
                    {
                        if (emptyProduct is null && stated > 0)
                            return Result<PurchaseOrderDto>.Failure($"No empty-cylinder product exists for {orderItem.Product.Name}, so the empties sent cannot be recorded.");
                        if (stated > available)
                            return Result<PurchaseOrderDto>.Failure($"Only {available} empty {emptyProduct?.Name} in stock, cannot send {stated} to the company.");
                        emptiesToSend = stated;
                    }
                    else
                    {
                        emptiesToSend = Math.Min(receiveItem.ReceivedQuantity, available);
                    }

                    if (emptiesToSend > 0 && emptyProduct is not null && emptyStock is not null)
                    {
                        emptyStock.Quantity -= emptiesToSend;

                        var ep = await _context.Products.FindAsync([emptyProduct.Id], cancellationToken);
                        if (ep is not null) ep.CurrentStock -= emptiesToSend;

                        orderItem.EmptySentQuantity += emptiesToSend;

                        await _context.StockMovements.AddAsync(new StockMovement
                        {
                            ProductId = emptyProduct.Id,
                            Type = StockMovementType.PurchaseReturnOut,
                            Quantity = emptiesToSend,
                            FromWarehouseId = entity.WarehouseId,
                            PurchaseOrderId = entity.Id,
                            Reference = $"{entity.OrderNumber} · empties to company",
                            MovementDate = DateTime.UtcNow
                        }, cancellationToken);
                    }
                }
            }
        }

        if (await SettleLeakagesAsync(entity, request.Leakages, cancellationToken) is string leakageError)
            return Result<PurchaseOrderDto>.Failure(leakageError);

        var totalOrdered = entity.Items.Sum(i => i.OrderedQuantity);
        var totalReceived = entity.Items.Sum(i => i.ReceivedQuantity);

        entity.Status = totalReceived >= totalOrdered
            ? PurchaseOrderStatus.Received
            : PurchaseOrderStatus.PartiallyReceived;

        if (commissionAccrued > 0 && supplier is not null)
        {
            entity.CommissionEarned += commissionAccrued;
            supplier.CommissionBalance += commissionAccrued;
        }

        entity.ReceivedDate = DateTime.UtcNow;
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
    /// Draws down the requested amount of the supplier's commission balance against this order's
    /// payable (goods + transportation), recording it on the order. The caller picks the amount —
    /// commission credit is optional and varies per order, not an automatic fixed deduction — so this
    /// only validates it's within what's actually available rather than silently capping it.
    /// Returns an error message, or null when the amount is sound.
    /// </summary>
    private static string? ApplyCommission(PurchaseOrder order, Supplier supplier, decimal requestedAmount)
    {
        if (requestedAmount == 0) return null;
        if (requestedAmount < 0) return "Commission credit cannot be negative.";

        var payable = order.TotalAmount + order.TransportationCost;
        var maxApplicable = Math.Min(supplier.CommissionBalance, payable);
        if (requestedAmount > maxApplicable)
            return $"Commission credit ({requestedAmount:N2}) exceeds what's available for this order ({maxApplicable:N2}).";

        order.CommissionApplied = requestedAmount;
        supplier.CommissionBalance -= requestedAmount;
        return null;
    }
}
