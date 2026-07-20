using AutoMapper;
using LpgErp.Application.Common.Interfaces;
using LpgErp.Application.Common.Models;
using LpgErp.Application.Features.Suppliers.DTOs;
using LpgErp.Domain.Entities;
using LpgErp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LpgErp.Application.Features.Suppliers;

public interface ISupplierService : IService<CreateSupplierRequest, UpdateSupplierRequest, SupplierDto> { }

public class SupplierService : ISupplierService
{
    private readonly IRepository<Supplier> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IApplicationDbContext _context;

    public SupplierService(IRepository<Supplier> repository, IUnitOfWork unitOfWork, IMapper mapper, IApplicationDbContext context)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _context = context;
    }

    public async Task<Result<PagedResult<SupplierDto>>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Suppliers
            .Where(s => !s.IsDeleted)
            .Include(s => s.Brand)
            .OrderBy(s => s.Name);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var pagedResult = new PagedResult<SupplierDto>
        {
            Items = _mapper.Map<IReadOnlyList<SupplierDto>>(items),
            Pagination = new PaginationMeta
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            }
        };

        return Result<PagedResult<SupplierDto>>.Success(pagedResult);
    }

    public async Task<Result<IReadOnlyList<SupplierDto>>> GetAllListAsync(CancellationToken cancellationToken = default)
    {
        var items = await _context.Suppliers
            .Where(s => !s.IsDeleted)
            .Include(s => s.Brand)
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<SupplierDto>>.Success(_mapper.Map<IReadOnlyList<SupplierDto>>(items));
    }

    public async Task<Result<SupplierDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Suppliers
            .Include(s => s.Brand)
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted, cancellationToken);

        if (entity is null)
            return Result<SupplierDto>.Failure("Supplier not found.");

        return Result<SupplierDto>.Success(_mapper.Map<SupplierDto>(entity));
    }

    public async Task<Result<SupplierDto>> CreateAsync(CreateSupplierRequest createDto, CancellationToken cancellationToken = default)
    {
        var entity = _mapper.Map<Supplier>(createDto);
        await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<SupplierDto>.Success(_mapper.Map<SupplierDto>(entity));
    }

    public async Task<Result<SupplierDto>> UpdateAsync(Guid id, UpdateSupplierRequest updateDto, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            return Result<SupplierDto>.Failure("Supplier not found.");

        _mapper.Map(updateDto, entity);
        _repository.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<SupplierDto>.Success(_mapper.Map<SupplierDto>(entity));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            return Result.Failure("Supplier not found.");

        _repository.Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
