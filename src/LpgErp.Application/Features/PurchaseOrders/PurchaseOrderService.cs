using AutoMapper;
using LpgErp.Application.Common.Interfaces;
using LpgErp.Application.Common.Models;
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
            .FirstOrDefaultAsync(po => po.Id == id && !po.IsDeleted, cancellationToken);

        if (entity is null) return Result<PurchaseOrderDto>.Failure("Purchase order not found.");
        return Result<PurchaseOrderDto>.Success(_mapper.Map<PurchaseOrderDto>(entity));
    }

    public async Task<Result<PurchaseOrderDto>> CreateAsync(CreatePurchaseOrderRequest request, CancellationToken cancellationToken = default)
    {
        var order = new PurchaseOrder
        {
            OrderNumber = $"PO-{DateTime.UtcNow:yyyyMMddHHmmss}",
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
                UnitPrice = i.UnitPrice
            }).ToList()
        };

        order.TotalAmount = order.Items.Sum(i => i.TotalPrice);

        var supplier = await _context.Suppliers.FirstOrDefaultAsync(s => s.Id == request.SupplierId && !s.IsDeleted, cancellationToken);
        if (supplier is null) return Result<PurchaseOrderDto>.Failure("Supplier not found.");
        ApplyCommission(order, supplier);

        await _context.PurchaseOrders.AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(order.Id, cancellationToken);
    }

    public async Task<Result<PurchaseOrderDto>> UpdateAsync(Guid id, UpdatePurchaseOrderRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _context.PurchaseOrders
            .Include(po => po.Items)
            .FirstOrDefaultAsync(po => po.Id == id && !po.IsDeleted, cancellationToken);

        if (entity is null) return Result<PurchaseOrderDto>.Failure("Purchase order not found.");
        if (entity.Status != PurchaseOrderStatus.Draft)
            return Result<PurchaseOrderDto>.Failure("Only draft orders can be updated.");

        // Refund any commission previously applied to the original supplier before the order is re-priced/re-assigned.
        if (entity.CommissionApplied > 0)
        {
            var originalSupplier = await _context.Suppliers.FindAsync([entity.SupplierId], cancellationToken);
            if (originalSupplier is not null) originalSupplier.CommissionBalance += entity.CommissionApplied;
            entity.CommissionApplied = 0;
        }

        entity.SupplierId = request.SupplierId;
        entity.WarehouseId = request.WarehouseId;
        entity.Status = request.Status;
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
            UnitPrice = i.UnitPrice
        }).ToList();

        entity.TotalAmount = entity.Items.Sum(i => i.TotalPrice);

        var supplier = await _context.Suppliers.FirstOrDefaultAsync(s => s.Id == request.SupplierId && !s.IsDeleted, cancellationToken);
        if (supplier is null) return Result<PurchaseOrderDto>.Failure("Supplier not found.");
        ApplyCommission(entity, supplier);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

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
        entity.OrderDate = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<Result<PurchaseOrderDto>> ReceiveAsync(Guid id, ReceivePurchaseOrderRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _context.PurchaseOrders
            .Include(po => po.Items)
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
                orderItem.ReceivedQuantity += receiveItem.ReceivedQuantity;
                orderItem.DamagedQuantity += receiveItem.DamagedQuantity;
                orderItem.MissingQuantity += receiveItem.MissingQuantity;

                var goodQuantity = receiveItem.ReceivedQuantity - receiveItem.DamagedQuantity;
                if (goodQuantity > 0)
                {
                    var stockLevel = await _context.StockLevels
                        .FirstOrDefaultAsync(s => s.WarehouseId == entity.WarehouseId && s.ProductId == orderItem.ProductId, cancellationToken);

                    if (stockLevel is null)
                    {
                        stockLevel = new StockLevel { WarehouseId = entity.WarehouseId, ProductId = orderItem.ProductId, Quantity = goodQuantity };
                        await _context.StockLevels.AddAsync(stockLevel, cancellationToken);
                    }
                    else
                    {
                        stockLevel.Quantity += goodQuantity;
                    }

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
            }
        }

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
    /// Draws down the supplier's outstanding commission balance against this order's payable
    /// (goods + transportation), recording the deducted amount on the order.
    /// </summary>
    private static void ApplyCommission(PurchaseOrder order, Supplier supplier)
    {
        var payable = order.TotalAmount + order.TransportationCost;
        var applied = Math.Min(supplier.CommissionBalance, payable);
        if (applied <= 0) return;

        order.CommissionApplied = applied;
        supplier.CommissionBalance -= applied;
    }
}
