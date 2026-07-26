using AutoMapper;
using LpgErp.Application.Common.Interfaces;
using LpgErp.Application.Common.Models;
using LpgErp.Application.Features.PriceHistories.DTOs;
using LpgErp.Domain.Entities;
using LpgErp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LpgErp.Application.Features.PriceHistories;

public interface IPriceHistoryService
{
    Task<Result<PagedResult<PriceHistoryDto>>> GetAllAsync(int pageNumber, int pageSize, CancellationToken ct = default);
    Task<Result<IReadOnlyList<PriceHistoryDto>>> GetByProductAsync(Guid productId, CancellationToken ct = default);
    Task<Result<PriceHistoryDto>> CreateAsync(CreatePriceHistoryRequest request, CancellationToken ct = default);
}

public class PriceHistoryService : IPriceHistoryService
{
    private readonly IApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public PriceHistoryService(IApplicationDbContext context, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<PriceHistoryDto>>> GetAllAsync(int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var query = _context.PriceHistories
            .Where(p => !p.IsDeleted)
            .Include(p => p.Product)
            .OrderByDescending(p => p.EffectiveDate);

        var total = await query.CountAsync(ct);
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        return Result<PagedResult<PriceHistoryDto>>.Success(new PagedResult<PriceHistoryDto>
        {
            Items = _mapper.Map<IReadOnlyList<PriceHistoryDto>>(items),
            Pagination = new PaginationMeta { PageNumber = pageNumber, PageSize = pageSize, TotalCount = total }
        });
    }

    public async Task<Result<IReadOnlyList<PriceHistoryDto>>> GetByProductAsync(Guid productId, CancellationToken ct = default)
    {
        var items = await _context.PriceHistories
            .Where(p => !p.IsDeleted && p.ProductId == productId)
            .Include(p => p.Product)
            .OrderByDescending(p => p.EffectiveDate)
            .ToListAsync(ct);

        return Result<IReadOnlyList<PriceHistoryDto>>.Success(_mapper.Map<IReadOnlyList<PriceHistoryDto>>(items));
    }

    public async Task<Result<PriceHistoryDto>> CreateAsync(CreatePriceHistoryRequest request, CancellationToken ct = default)
    {
        var product = await _context.Products.FindAsync([request.ProductId], ct);
        if (product is null) return Result<PriceHistoryDto>.Failure("Product not found.");

        var previousPrice = request.PriceType == PriceType.Purchase ? product.PurchasePrice : product.SalePrice;

        var entity = _mapper.Map<PriceHistory>(request);
        entity.PreviousPrice = previousPrice;

        if (request.PriceType == PriceType.Purchase)
            product.PurchasePrice = request.NewPrice;
        else
            product.SalePrice = request.NewPrice;

        await _context.PriceHistories.AddAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var result = await _context.PriceHistories.Include(p => p.Product).FirstOrDefaultAsync(p => p.Id == entity.Id, ct);
        return Result<PriceHistoryDto>.Success(_mapper.Map<PriceHistoryDto>(result));
    }

    public static void RecordPriceChange(IApplicationDbContext context, Product product, PriceType priceType, decimal newPrice, PriceChangeReason reason, string? reference = null)
    {
        var previousPrice = priceType == PriceType.Purchase ? product.PurchasePrice : product.SalePrice;

        if (previousPrice == newPrice) return;

        context.PriceHistories.Add(new PriceHistory
        {
            ProductId = product.Id,
            PriceType = priceType,
            PreviousPrice = previousPrice,
            NewPrice = newPrice,
            Reason = reason,
            EffectiveDate = DateTime.UtcNow,
            Reference = reference
        });
    }
}
