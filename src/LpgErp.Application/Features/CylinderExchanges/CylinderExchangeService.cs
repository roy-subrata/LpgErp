using AutoMapper;
using LpgErp.Application.Common.Interfaces;
using LpgErp.Application.Common.Models;
using LpgErp.Application.Features.CylinderExchanges.DTOs;
using LpgErp.Application.Features.Payments;
using LpgErp.Domain.Entities;
using LpgErp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LpgErp.Application.Features.CylinderExchanges;

public interface ICylinderExchangeService
{
    Task<Result<PagedResult<CylinderExchangeDto>>> GetAllAsync(int pageNumber, int pageSize, CancellationToken ct = default);
    Task<Result<CylinderExchangeDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<CylinderExchangeDto>> CreateAsync(CreateCylinderExchangeRequest request, CancellationToken ct = default);
    Task<Result<CylinderExchangeDto>> UpdateAsync(Guid id, UpdateCylinderExchangeRequest request, CancellationToken ct = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken ct = default);
}

/// <summary>
/// A cylinder exchange swaps one brand's empty cylinder for another's. Cylinders are an asset in their
/// own right, so the swap moves real stock: the incoming brand's empties come in, the outgoing brand's
/// go out. Gas is untouched — only the steel changes hands.
/// </summary>
public class CylinderExchangeService : ICylinderExchangeService
{
    private readonly IApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CylinderExchangeService(IApplicationDbContext context, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    private IQueryable<CylinderExchange> WithRelations() => _context.CylinderExchanges
        .Include(e => e.Customer).Include(e => e.Warehouse)
        .Include(e => e.IncomingBrand).Include(e => e.IncomingCylinderSize)
        .Include(e => e.OutgoingBrand).Include(e => e.OutgoingCylinderSize);

    public async Task<Result<PagedResult<CylinderExchangeDto>>> GetAllAsync(int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var query = WithRelations().Where(e => !e.IsDeleted).OrderByDescending(e => e.ExchangeDate);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return Result<PagedResult<CylinderExchangeDto>>.Success(new PagedResult<CylinderExchangeDto>
        {
            Items = _mapper.Map<IReadOnlyList<CylinderExchangeDto>>(items),
            Pagination = new PaginationMeta { PageNumber = pageNumber, PageSize = pageSize, TotalCount = total }
        });
    }

    public async Task<Result<CylinderExchangeDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await WithRelations().FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted, ct);
        if (entity is null) return Result<CylinderExchangeDto>.Failure("Cylinder exchange not found.");
        return Result<CylinderExchangeDto>.Success(_mapper.Map<CylinderExchangeDto>(entity));
    }

    public async Task<Result<CylinderExchangeDto>> CreateAsync(CreateCylinderExchangeRequest request, CancellationToken ct = default)
    {
        if (request.ExchangeCharge > 0
            && await PaymentAccountRules.ValidateAsync(_context, request.PaymentAccountId, request.Method, ct) is string accountError)
            return Result<CylinderExchangeDto>.Failure(accountError);

        var entity = _mapper.Map<CylinderExchange>(request);
        entity.Id = Guid.NewGuid();
        entity.ExchangeDate = DateTime.UtcNow;

        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            // Validate availability inside the transaction to prevent TOCTOU races.
            var resolved = await ResolveAsync(request.WarehouseId,
                request.IncomingBrandId, request.IncomingCylinderSizeId, request.IncomingQuantity,
                request.OutgoingBrandId, request.OutgoingCylinderSizeId, request.OutgoingQuantity, ct);
            if (!resolved.IsSuccess)
            {
                await _unitOfWork.RollbackTransactionAsync(ct);
                return Result<CylinderExchangeDto>.Failure(resolved.Error!);
            }

            await ApplyStockAsync(entity, resolved.Data!, +1, ct);
            await _context.CylinderExchanges.AddAsync(entity, ct);

            // The swap fee is revenue collected at the counter, so it belongs in the cash and
            // wallet balances like any other payment.
            if (request.ExchangeCharge > 0)
            {
                await _context.Payments.AddAsync(new Payment
                {
                    CustomerId = request.CustomerId,
                    CylinderExchange = entity,
                    Purpose = PaymentPurpose.ExchangeCharge,
                    Direction = PaymentDirection.Inbound,
                    Method = request.Method,
                    PaymentAccountId = request.PaymentAccountId,
                    Amount = request.ExchangeCharge,
                    PaymentDate = entity.ExchangeDate,
                    Notes = "Cylinder exchange charge.",
                }, ct);
            }

            await _unitOfWork.SaveChangesAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            throw;
        }

        return await GetByIdAsync(entity.Id, ct);
    }

    public async Task<Result<CylinderExchangeDto>> UpdateAsync(Guid id, UpdateCylinderExchangeRequest request, CancellationToken ct = default)
    {
        var entity = await _context.CylinderExchanges.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted, ct);
        if (entity is null) return Result<CylinderExchangeDto>.Failure("Cylinder exchange not found.");

        // Reverse the original movement and apply the new one, so an edit cannot leave stock adrift.
        var original = await ResolveAsync(entity.WarehouseId,
            entity.IncomingBrandId, entity.IncomingCylinderSizeId, entity.IncomingQuantity,
            entity.OutgoingBrandId, entity.OutgoingCylinderSizeId, entity.OutgoingQuantity, ct, validateAvailability: false);
        if (!original.IsSuccess) return Result<CylinderExchangeDto>.Failure(original.Error!);

        var updated = await ResolveAsync(request.WarehouseId,
            request.IncomingBrandId, request.IncomingCylinderSizeId, request.IncomingQuantity,
            request.OutgoingBrandId, request.OutgoingCylinderSizeId, request.OutgoingQuantity, ct);
        if (!updated.IsSuccess) return Result<CylinderExchangeDto>.Failure(updated.Error!);

        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            await ApplyStockAsync(entity, original.Data!, -1, ct);

            _mapper.Map(request, entity);
            await ApplyStockAsync(entity, updated.Data!, +1, ct);

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

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _context.CylinderExchanges.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted, ct);
        if (entity is null) return Result.Failure("Cylinder exchange not found.");

        var resolved = await ResolveAsync(entity.WarehouseId,
            entity.IncomingBrandId, entity.IncomingCylinderSizeId, entity.IncomingQuantity,
            entity.OutgoingBrandId, entity.OutgoingCylinderSizeId, entity.OutgoingQuantity, ct, validateAvailability: false);
        if (!resolved.IsSuccess) return Result.Failure(resolved.Error!);

        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            // The exchange never happened: the incoming cylinders go back out, the outgoing ones return.
            await ApplyStockAsync(entity, resolved.Data!, -1, ct);

            var linkedPayment = await _context.Payments
                .FirstOrDefaultAsync(p => p.CylinderExchangeId == id && !p.IsDeleted, ct);
            if (linkedPayment is not null) _context.Payments.Remove(linkedPayment);

            _context.CylinderExchanges.Remove(entity);
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

    private sealed record ExchangeProducts(Guid IncomingProductId, int IncomingQuantity, Guid OutgoingProductId, int OutgoingQuantity);

    /// <summary>
    /// Maps each side of the swap onto the empty-cylinder product for that brand and size, and checks the
    /// warehouse can actually supply the outgoing cylinders. Reversals skip the availability check —
    /// undoing a movement never needs stock to be on hand.
    /// </summary>
    private async Task<Result<ExchangeProducts>> ResolveAsync(
        Guid warehouseId,
        Guid incomingBrandId, Guid incomingSizeId, int incomingQuantity,
        Guid outgoingBrandId, Guid outgoingSizeId, int outgoingQuantity,
        CancellationToken ct, bool validateAvailability = true)
    {
        if (incomingQuantity <= 0 || outgoingQuantity <= 0)
            return Result<ExchangeProducts>.Failure("Incoming and outgoing quantities must be greater than zero.");

        if (!await _context.Warehouses.AnyAsync(w => w.Id == warehouseId && !w.IsDeleted, ct))
            return Result<ExchangeProducts>.Failure("Warehouse not found.");

        var incoming = await FindEmptyCylinderProductAsync(incomingBrandId, incomingSizeId, ct);
        if (incoming is null)
            return Result<ExchangeProducts>.Failure("No empty-cylinder product exists for the incoming brand and size.");

        var outgoing = await FindEmptyCylinderProductAsync(outgoingBrandId, outgoingSizeId, ct);
        if (outgoing is null)
            return Result<ExchangeProducts>.Failure("No empty-cylinder product exists for the outgoing brand and size.");

        if (incoming.Id == outgoing.Id)
            return Result<ExchangeProducts>.Failure("An exchange must swap different cylinders.");

        if (validateAvailability)
        {
            var stock = await _context.StockLevels
                .FirstOrDefaultAsync(s => s.WarehouseId == warehouseId && s.ProductId == outgoing.Id, ct);
            if (stock is null || stock.Quantity < outgoingQuantity)
                return Result<ExchangeProducts>.Failure($"Insufficient stock of {outgoing.Name} in the warehouse.");
        }

        return Result<ExchangeProducts>.Success(new ExchangeProducts(incoming.Id, incomingQuantity, outgoing.Id, outgoingQuantity));
    }

    private Task<Product?> FindEmptyCylinderProductAsync(Guid brandId, Guid sizeId, CancellationToken ct) =>
        _context.Products.FirstOrDefaultAsync(p => !p.IsDeleted
            && p.Type == ProductType.EmptyCylinder
            && p.BrandId == brandId
            && p.CylinderSizeId == sizeId, ct);

    /// <summary>Moves the stock for one exchange. <paramref name="sign"/> is +1 to apply, -1 to reverse.</summary>
    private async Task ApplyStockAsync(CylinderExchange exchange, ExchangeProducts products, int sign, CancellationToken ct)
    {
        var reference = $"CX-{exchange.Id.ToString()[..8].ToUpperInvariant()}";

        await MoveAsync(products.IncomingProductId, sign * products.IncomingQuantity, reference, exchange.WarehouseId, ct);
        await MoveAsync(products.OutgoingProductId, -sign * products.OutgoingQuantity, reference, exchange.WarehouseId, ct);
    }

    private async Task MoveAsync(Guid productId, int delta, string reference, Guid warehouseId, CancellationToken ct)
    {
        if (delta == 0) return;

        var stock = await _context.StockLevels
            .FirstOrDefaultAsync(s => s.WarehouseId == warehouseId && s.ProductId == productId, ct);
        if (stock is null)
        {
            stock = new StockLevel { WarehouseId = warehouseId, ProductId = productId, Quantity = 0 };
            await _context.StockLevels.AddAsync(stock, ct);
        }
        stock.Quantity += delta;

        var product = await _context.Products.FindAsync([productId], ct);
        if (product is not null) product.CurrentStock += delta;

        await _context.StockMovements.AddAsync(new StockMovement
        {
            ProductId = productId,
            Type = delta > 0 ? StockMovementType.Return : StockMovementType.SaleOut,
            Quantity = Math.Abs(delta),
            FromWarehouseId = delta > 0 ? null : warehouseId,
            ToWarehouseId = delta > 0 ? warehouseId : null,
            Reference = reference,
            MovementDate = DateTime.UtcNow
        }, ct);
    }
}
