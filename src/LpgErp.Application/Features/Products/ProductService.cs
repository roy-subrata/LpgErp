using AutoMapper;
using LpgErp.Application.Common.Interfaces;
using LpgErp.Application.Common.Models;
using LpgErp.Application.Features.Products.DTOs;
using LpgErp.Domain.Entities;
using LpgErp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LpgErp.Application.Features.Products;

public interface IProductService : IService<CreateProductRequest, UpdateProductRequest, ProductDto> { }

public class ProductService : IProductService
{
    private readonly IRepository<Product> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IApplicationDbContext _context;

    public ProductService(IRepository<Product> repository, IUnitOfWork unitOfWork, IMapper mapper, IApplicationDbContext context)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _context = context;
    }

    public async Task<Result<PagedResult<ProductDto>>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Products
            .Where(p => !p.IsDeleted)
            .Include(p => p.Brand)
            .Include(p => p.CylinderSize)
            .OrderBy(p => p.Name);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var pagedResult = new PagedResult<ProductDto>
        {
            Items = _mapper.Map<IReadOnlyList<ProductDto>>(items),
            Pagination = new PaginationMeta
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            }
        };

        return Result<PagedResult<ProductDto>>.Success(pagedResult);
    }

    public async Task<Result<IReadOnlyList<ProductDto>>> GetAllListAsync(CancellationToken cancellationToken = default)
    {
        var items = await _context.Products
            .Where(p => !p.IsDeleted)
            .Include(p => p.Brand)
            .Include(p => p.CylinderSize)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<ProductDto>>.Success(_mapper.Map<IReadOnlyList<ProductDto>>(items));
    }

    public async Task<Result<ProductDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Products
            .Include(p => p.Brand)
            .Include(p => p.CylinderSize)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, cancellationToken);

        if (entity is null)
            return Result<ProductDto>.Failure("Product not found.");

        return Result<ProductDto>.Success(_mapper.Map<ProductDto>(entity));
    }

    public async Task<Result<ProductDto>> CreateAsync(CreateProductRequest createDto, CancellationToken cancellationToken = default)
    {
        var entity = _mapper.Map<Product>(createDto);
        await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<ProductDto>.Success(_mapper.Map<ProductDto>(entity));
    }

    public async Task<Result<ProductDto>> UpdateAsync(Guid id, UpdateProductRequest updateDto, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            return Result<ProductDto>.Failure("Product not found.");

        _mapper.Map(updateDto, entity);
        _repository.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<ProductDto>.Success(_mapper.Map<ProductDto>(entity));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            return Result.Failure("Product not found.");

        _repository.Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
