using LpgErp.Application.Common.Interfaces;
using LpgErp.Application.Common.Models;
using LpgErp.Domain.Entities;
using LpgErp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LpgErp.Application.Features.StockTransfer;

public record StockTransferRequest(Guid ProductId, Guid FromWarehouseId, Guid ToWarehouseId, int Quantity, string? Reference);
public record StockTransferResponse(Guid MovementId, int Quantity, string ProductName, string FromWarehouseName, string ToWarehouseName);

public interface IStockTransferService
{
    Task<Result<StockTransferResponse>> TransferAsync(StockTransferRequest request, CancellationToken ct = default);
    Task<Result<PagedResult<StockMovementDto>>> GetHistoryAsync(int pageNumber, int pageSize, CancellationToken ct = default);
}

public class StockTransferService : IStockTransferService
{
    private readonly IApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public StockTransferService(IApplicationDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<StockTransferResponse>> TransferAsync(StockTransferRequest request, CancellationToken ct = default)
    {
        var product = await _context.Products.FindAsync([request.ProductId], ct);
        if (product is null) return Result<StockTransferResponse>.Failure("Product not found.");

        var fromStock = await _context.StockLevels.FirstOrDefaultAsync(s => s.WarehouseId == request.FromWarehouseId && s.ProductId == request.ProductId, ct);
        if (fromStock is null || fromStock.Quantity < request.Quantity)
            return Result<StockTransferResponse>.Failure("Insufficient stock in source warehouse.");

        var toStock = await _context.StockLevels.FirstOrDefaultAsync(s => s.WarehouseId == request.ToWarehouseId && s.ProductId == request.ProductId, ct);

        fromStock.Quantity -= request.Quantity;
        if (toStock is null)
        {
            toStock = new StockLevel { WarehouseId = request.ToWarehouseId, ProductId = request.ProductId, Quantity = request.Quantity };
            await _context.StockLevels.AddAsync(toStock, ct);
        }
        else
        {
            toStock.Quantity += request.Quantity;
        }

        var movement = new StockMovement
        {
            ProductId = request.ProductId,
            Type = StockMovementType.TransferOut,
            Quantity = request.Quantity,
            FromWarehouseId = request.FromWarehouseId,
            ToWarehouseId = request.ToWarehouseId,
            Reference = request.Reference,
            MovementDate = DateTime.UtcNow
        };
        await _context.StockMovements.AddAsync(movement, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var fromWarehouse = await _context.Warehouses.FindAsync([request.FromWarehouseId], ct);
        var toWarehouse = await _context.Warehouses.FindAsync([request.ToWarehouseId], ct);

        return Result<StockTransferResponse>.Success(new StockTransferResponse(
            movement.Id, request.Quantity, product.Name,
            fromWarehouse?.Name ?? "", toWarehouse?.Name ?? ""));
    }

    public async Task<Result<PagedResult<StockMovementDto>>> GetHistoryAsync(int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var query = _context.StockMovements.Where(m => !m.IsDeleted)
            .Include(m => m.Product).Include(m => m.FromWarehouse).Include(m => m.ToWarehouse)
            .OrderByDescending(m => m.MovementDate);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        var dtos = items.Select(m => new StockMovementDto
        {
            Id = m.Id, ProductId = m.ProductId, ProductName = m.Product.Name,
            Type = m.Type, Quantity = m.Quantity,
            FromWarehouseId = m.FromWarehouseId, FromWarehouseName = m.FromWarehouse?.Name,
            ToWarehouseId = m.ToWarehouseId, ToWarehouseName = m.ToWarehouse?.Name,
            Reference = m.Reference, MovementDate = m.MovementDate
        }).ToList();

        return Result<PagedResult<StockMovementDto>>.Success(new PagedResult<StockMovementDto>
        {
            Items = dtos,
            Pagination = new PaginationMeta { PageNumber = pageNumber, PageSize = pageSize, TotalCount = total }
        });
    }
}

public class StockMovementDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string? ProductName { get; set; }
    public StockMovementType Type { get; set; }
    public int Quantity { get; set; }
    public Guid? FromWarehouseId { get; set; }
    public string? FromWarehouseName { get; set; }
    public Guid? ToWarehouseId { get; set; }
    public string? ToWarehouseName { get; set; }
    public string? Reference { get; set; }
    public DateTime MovementDate { get; set; }
}
