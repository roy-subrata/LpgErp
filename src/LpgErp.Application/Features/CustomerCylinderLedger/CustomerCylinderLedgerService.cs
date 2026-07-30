using AutoMapper;
using LpgErp.Application.Common.Interfaces;
using LpgErp.Application.Common.Mappings;
using LpgErp.Application.Common.Models;
using LpgErp.Application.Features.Payments.DTOs;
using LpgErp.Application.Features.SalesOrders;
using LpgErp.Application.Features.SalesOrders.DTOs;
using LpgErp.Domain.Entities;
using LpgErp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LpgErp.Application.Features.CustomerCylinderLedger;

public interface ICustomerCylinderLedgerService
{
    Task<Result<IReadOnlyList<CustomerCylinderBalanceDto>>> GetByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<Result<PagedResult<CustomerCylinderBalanceDto>>> GetAllPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<Result<CustomerCylinderBalanceDto>> AdjustBalanceAsync(AdjustCylinderBalanceRequest request, CancellationToken cancellationToken = default);
    Task<Result<ForfeitCylinderResultDto>> ForfeitAsync(ForfeitCylinderRequest request, CancellationToken cancellationToken = default);
}

public class CustomerCylinderLedgerService : ICustomerCylinderLedgerService
{
    private readonly IApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISalesOrderService _salesOrderService;
    private readonly IMapper _mapper;

    public CustomerCylinderLedgerService(
        IApplicationDbContext context,
        IUnitOfWork unitOfWork,
        ISalesOrderService salesOrderService,
        IMapper mapper)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _salesOrderService = salesOrderService;
        _mapper = mapper;
    }

    public async Task<Result<IReadOnlyList<CustomerCylinderBalanceDto>>> GetByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var items = await _context.CustomerCylinderBalances
            .Where(c => c.CustomerId == customerId && !c.IsDeleted)
            .Include(c => c.Customer)
            .Include(c => c.Brand)
            .Include(c => c.CylinderSize)
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<CustomerCylinderBalanceDto>>.Success(
            _mapper.Map<IReadOnlyList<CustomerCylinderBalanceDto>>(items));
    }

    public async Task<Result<PagedResult<CustomerCylinderBalanceDto>>> GetAllPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.CustomerCylinderBalances
            .Where(c => !c.IsDeleted)
            .Include(c => c.Customer)
            .Include(c => c.Brand)
            .Include(c => c.CylinderSize)
            .OrderBy(c => c.Customer.Name);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return Result<PagedResult<CustomerCylinderBalanceDto>>.Success(new PagedResult<CustomerCylinderBalanceDto>
        {
            Items = _mapper.Map<IReadOnlyList<CustomerCylinderBalanceDto>>(items),
            Pagination = new PaginationMeta { PageNumber = pageNumber, PageSize = pageSize, TotalCount = totalCount }
        });
    }

    public async Task<Result<CustomerCylinderBalanceDto>> AdjustBalanceAsync(AdjustCylinderBalanceRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Quantity <= 0)
            return Result<CustomerCylinderBalanceDto>.Failure("Quantity must be positive.");

        // Optional monetary settlement: the returned cylinders reduce a credit sales order's due.
        Payment? settlement = null;
        if (request.SettlementAmount > 0)
        {
            if (!request.IsReturn)
                return Result<CustomerCylinderBalanceDto>.Failure("A credit settlement applies only to cylinder returns.");
            if (request.SettlementSalesOrderId is null)
                return Result<CustomerCylinderBalanceDto>.Failure("Select the credit sales order to settle against.");

            var order = await _context.SalesOrders
                .FirstOrDefaultAsync(o => o.Id == request.SettlementSalesOrderId && !o.IsDeleted, cancellationToken);
            if (order is null || order.CustomerId != request.CustomerId)
                return Result<CustomerCylinderBalanceDto>.Failure("Sales order not found for this customer.");
            if (order.Status != SalesOrderStatus.Delivered || !order.IsCreditSale)
                return Result<CustomerCylinderBalanceDto>.Failure("Settlements apply to delivered credit sales orders.");

            var paid = await _context.Payments
                .Where(p => !p.IsDeleted && p.SalesOrderId == order.Id && p.Direction == PaymentDirection.Inbound)
                .SumAsync(p => p.Amount, cancellationToken);
            var due = order.TotalAmount - order.Discount - paid;
            if (request.SettlementAmount > due)
                return Result<CustomerCylinderBalanceDto>.Failure($"Settlement exceeds the order's remaining due of ৳{due:N0}.");

            settlement = new Payment
            {
                SalesOrderId = order.Id,
                Direction = PaymentDirection.Inbound,
                Method = PaymentMethod.Cash,
                Amount = request.SettlementAmount,
                PaymentDate = DateTime.UtcNow,
                Reference = "CYL-RETURN",
                Notes = $"Credit adjusted for {request.Quantity} returned cylinder(s)."
            };
        }

        CustomerCylinderBalance? balance = null;

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // Load balance inside transaction to prevent TOCTOU race on outstanding check.
            balance = await _context.CustomerCylinderBalances
                .Include(c => c.Customer)
                .Include(c => c.Brand)
                .Include(c => c.CylinderSize)
                .FirstOrDefaultAsync(c => c.CustomerId == request.CustomerId
                    && c.BrandId == request.BrandId
                    && c.CylinderSizeId == request.CylinderSizeId
                    && !c.IsDeleted, cancellationToken);

            if (request.IsReturn && request.Quantity > (balance is null ? 0 : balance.Received - balance.Returned - balance.Forfeited))
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result<CustomerCylinderBalanceDto>.Failure("Cannot return more cylinders than are outstanding.");
            }

            if (balance is null)
            {
                balance = new CustomerCylinderBalance
                {
                    CustomerId = request.CustomerId,
                    BrandId = request.BrandId,
                    CylinderSizeId = request.CylinderSizeId,
                    Received = 0,
                    Returned = 0
                };
                await _context.CustomerCylinderBalances.AddAsync(balance, cancellationToken);
            }

            if (request.IsReturn)
                balance.Returned += request.Quantity;
            else
                balance.Received += request.Quantity;

            if (settlement is not null)
                await _context.Payments.AddAsync(settlement, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }

        return Result<CustomerCylinderBalanceDto>.Success(_mapper.Map<CustomerCylinderBalanceDto>(balance));
    }

    /// <summary>
    /// Closes out outstanding cylinders the customer is keeping for good: bills them for it as a
    /// normal sale of the empty cylinder (billed now or on credit, same as any other sale), and moves
    /// the quantity from "outstanding" to "forfeited" rather than "returned" — nothing physically
    /// came back. Delegating to the sales flow gives this a proper receivable, ageing entry, and
    /// audit trail instead of a bespoke money record.
    /// </summary>
    public async Task<Result<ForfeitCylinderResultDto>> ForfeitAsync(ForfeitCylinderRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Quantity <= 0)
            return Result<ForfeitCylinderResultDto>.Failure("Quantity must be positive.");
        if (request.UnitPrice <= 0)
            return Result<ForfeitCylinderResultDto>.Failure("Unit price must be positive.");

        var balance = await _context.CustomerCylinderBalances
            .Include(b => b.Customer).Include(b => b.Brand).Include(b => b.CylinderSize)
            .FirstOrDefaultAsync(b => b.CustomerId == request.CustomerId
                && b.BrandId == request.BrandId
                && b.CylinderSizeId == request.CylinderSizeId
                && !b.IsDeleted, cancellationToken);

        var outstanding = balance is null ? 0 : balance.Received - balance.Returned - balance.Forfeited;
        if (request.Quantity > outstanding)
            return Result<ForfeitCylinderResultDto>.Failure($"Only {outstanding} cylinder(s) outstanding — cannot forfeit {request.Quantity}.");

        var emptyProduct = await _context.Products.FirstOrDefaultAsync(p => !p.IsDeleted
            && p.Type == ProductType.EmptyCylinder
            && p.BrandId == request.BrandId
            && p.CylinderSizeId == request.CylinderSizeId, cancellationToken);
        if (emptyProduct is null)
            return Result<ForfeitCylinderResultDto>.Failure("No empty-cylinder product exists for this brand and size — add one before forfeiting.");

        var customer = balance!.Customer;

        var created = await _salesOrderService.CreateAsync(new CreateSalesOrderRequest
        {
            CustomerId = request.CustomerId,
            WarehouseId = request.WarehouseId,
            IsCreditSale = request.IsCreditSale,
            DueDate = request.IsCreditSale ? DateTime.UtcNow.AddDays(customer.PaymentDueDays) : null,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? "Cylinder forfeit" : $"Cylinder forfeit — {request.Notes}",
            Items = [new CreateSalesOrderItemRequest { ProductId = emptyProduct.Id, Quantity = request.Quantity, UnitPrice = request.UnitPrice }],
            Payment = request.IsCreditSale ? null : new OrderPaymentRequest { Amount = request.Quantity * request.UnitPrice },
        }, cancellationToken);
        if (!created.IsSuccess) return Result<ForfeitCylinderResultDto>.Failure(created.Error!);

        var confirmed = await _salesOrderService.ConfirmAsync(created.Data!.Id, cancellationToken);
        if (!confirmed.IsSuccess) return Result<ForfeitCylinderResultDto>.Failure(confirmed.Error!);

        var delivered = await _salesOrderService.DeliverAsync(created.Data.Id, cancellationToken);
        if (!delivered.IsSuccess)
            return Result<ForfeitCylinderResultDto>.Failure(
                $"Order {created.Data.OrderNumber} was created but could not be delivered: {delivered.Error}");

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // Delivery deducted the empty-cylinder product's warehouse stock as though this were a
            // fresh shipment out — it isn't. This cylinder left the warehouse long ago, uncounted,
            // when it was first lent out; forfeiting it now is a money event, not a stock movement.
            // Undo the incidental deduction so warehouse stock reflects what's physically still there.
            var stock = await _context.StockLevels
                .FirstOrDefaultAsync(s => s.WarehouseId == request.WarehouseId && s.ProductId == emptyProduct.Id, cancellationToken);
            if (stock is null)
            {
                stock = new StockLevel { WarehouseId = request.WarehouseId, ProductId = emptyProduct.Id, Quantity = 0 };
                await _context.StockLevels.AddAsync(stock, cancellationToken);
            }
            stock.Quantity += request.Quantity;

            var stored = await _context.Products.FindAsync([emptyProduct.Id], cancellationToken);
            if (stored is not null) stored.CurrentStock += request.Quantity;

            await _context.StockMovements.AddAsync(new StockMovement
            {
                ProductId = emptyProduct.Id,
                Type = StockMovementType.Adjustment,
                Quantity = request.Quantity,
                ToWarehouseId = request.WarehouseId,
                SalesOrderId = created.Data.Id,
                Reference = $"{created.Data.OrderNumber} · forfeit stock correction (no physical movement)",
                MovementDate = DateTime.UtcNow
            }, cancellationToken);

            // Re-load inside the transaction and re-validate outstanding — the sale above (its own,
            // separate transactions) took real time, during which a concurrent return or forfeit
            // against this same balance could have landed. Applying blindly here is exactly the
            // TOCTOU gap AdjustBalanceAsync already guards against; this closes the same gap for the
            // one field that check above couldn't protect, since it ran before the sale existed.
            var trackedBalance = await _context.CustomerCylinderBalances
                .Include(b => b.Customer).Include(b => b.Brand).Include(b => b.CylinderSize)
                .FirstOrDefaultAsync(b => b.Id == balance.Id, cancellationToken);
            var stillOutstanding = trackedBalance!.Received - trackedBalance.Returned - trackedBalance.Forfeited;
            if (request.Quantity > stillOutstanding)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result<ForfeitCylinderResultDto>.Failure(
                    $"Order {created.Data.OrderNumber} was created and charged, but the outstanding balance changed in the " +
                    $"meantime (only {stillOutstanding} left) — the ledger could not be updated. Reconcile manually.");
            }
            trackedBalance.Forfeited += request.Quantity;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result<ForfeitCylinderResultDto>.Success(new ForfeitCylinderResultDto
            {
                Balance = _mapper.Map<CustomerCylinderBalanceDto>(trackedBalance),
                SalesOrderId = created.Data.Id,
                SalesOrderNumber = created.Data.OrderNumber,
            });
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

public class CustomerCylinderBalanceDto : IMapFrom<CustomerCylinderBalance>
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Guid BrandId { get; set; }
    public string BrandName { get; set; } = string.Empty;
    public Guid CylinderSizeId { get; set; }
    public string CylinderSizeName { get; set; } = string.Empty;
    public int Received { get; set; }
    public int Returned { get; set; }
    public int Forfeited { get; set; }
    public int Outstanding { get; set; }
}

public class AdjustCylinderBalanceRequest
{
    public Guid CustomerId { get; set; }
    public Guid BrandId { get; set; }
    public Guid CylinderSizeId { get; set; }
    public int Quantity { get; set; }
    public bool IsReturn { get; set; }
    /// <summary>Optional: credit sales order whose due is reduced by this return.</summary>
    public Guid? SettlementSalesOrderId { get; set; }
    /// <summary>Optional: amount credited against the order when cylinders are returned.</summary>
    public decimal SettlementAmount { get; set; }
}

/// <summary>
/// The customer keeps outstanding cylinders instead of returning them, and pays for them instead —
/// a normal sale of the empty cylinder itself, billed now or on credit like any other sale.
/// </summary>
public class ForfeitCylinderRequest
{
    public Guid CustomerId { get; set; }
    public Guid BrandId { get; set; }
    public Guid CylinderSizeId { get; set; }
    public Guid WarehouseId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public bool IsCreditSale { get; set; }
    public string? Notes { get; set; }
}

public class ForfeitCylinderResultDto
{
    public CustomerCylinderBalanceDto Balance { get; set; } = new();
    public Guid SalesOrderId { get; set; }
    public string SalesOrderNumber { get; set; } = string.Empty;
}
