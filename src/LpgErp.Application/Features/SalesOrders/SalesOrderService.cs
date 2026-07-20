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
            OrderDate = DateTime.UtcNow,
            Items = request.Items.Select(i => new SalesOrderItem
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                CylinderExchangeQuantity = i.CylinderExchangeQuantity
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

        _context.SalesOrderItems.RemoveRange(entity.Items);
        entity.Items = request.Items.Select(i => new SalesOrderItem
        {
            SalesOrderId = id,
            ProductId = i.ProductId,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            CylinderExchangeQuantity = i.CylinderExchangeQuantity
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

        foreach (var item in entity.Items)
        {
            var stockLevel = await _context.StockLevels
                .FirstOrDefaultAsync(s => s.WarehouseId == entity.WarehouseId && s.ProductId == item.ProductId, cancellationToken);

            if (stockLevel is null || stockLevel.Quantity < item.Quantity)
                return Result<SalesOrderDto>.Failure($"Insufficient stock for product {item.ProductId} in warehouse.");

            stockLevel.Quantity -= item.Quantity;

            var product = await _context.Products.FindAsync([item.ProductId], cancellationToken);
            if (product is not null) product.CurrentStock -= item.Quantity;

            await _context.StockMovements.AddAsync(new StockMovement
            {
                ProductId = item.ProductId,
                Type = StockMovementType.SaleOut,
                Quantity = item.Quantity,
                FromWarehouseId = entity.WarehouseId,
                SalesOrderId = entity.Id,
                Reference = entity.OrderNumber,
                MovementDate = DateTime.UtcNow
            }, cancellationToken);
        }

        entity.Status = SalesOrderStatus.Delivered;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }
}
