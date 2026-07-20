using AutoMapper;
using LpgErp.Application.Common.Interfaces;
using LpgErp.Application.Common.Models;
using LpgErp.Application.Features.CylinderSizes.DTOs;
using LpgErp.Domain.Entities;
using LpgErp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LpgErp.Application.Features.CylinderSizes;

public interface ICylinderSizeService : IService<CreateCylinderSizeRequest, UpdateCylinderSizeRequest, CylinderSizeDto> { }

public class CylinderSizeService : ICylinderSizeService
{
    private readonly IRepository<CylinderSize> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IApplicationDbContext _context;

    public CylinderSizeService(IRepository<CylinderSize> repository, IUnitOfWork unitOfWork, IMapper mapper, IApplicationDbContext context)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _context = context;
    }

    public async Task<Result<PagedResult<CylinderSizeDto>>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.CylinderSizes
            .Where(cs => !cs.IsDeleted)
            .Include(cs => cs.Brand)
            .OrderBy(cs => cs.Name);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var pagedResult = new PagedResult<CylinderSizeDto>
        {
            Items = _mapper.Map<IReadOnlyList<CylinderSizeDto>>(items),
            Pagination = new PaginationMeta
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            }
        };

        return Result<PagedResult<CylinderSizeDto>>.Success(pagedResult);
    }

    public async Task<Result<IReadOnlyList<CylinderSizeDto>>> GetAllListAsync(CancellationToken cancellationToken = default)
    {
        var items = await _context.CylinderSizes
            .Where(cs => !cs.IsDeleted)
            .Include(cs => cs.Brand)
            .OrderBy(cs => cs.Name)
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<CylinderSizeDto>>.Success(_mapper.Map<IReadOnlyList<CylinderSizeDto>>(items));
    }

    public async Task<Result<CylinderSizeDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.CylinderSizes
            .Include(cs => cs.Brand)
            .FirstOrDefaultAsync(cs => cs.Id == id && !cs.IsDeleted, cancellationToken);

        if (entity is null)
            return Result<CylinderSizeDto>.Failure("Cylinder size not found.");

        return Result<CylinderSizeDto>.Success(_mapper.Map<CylinderSizeDto>(entity));
    }

    public async Task<Result<CylinderSizeDto>> CreateAsync(CreateCylinderSizeRequest createDto, CancellationToken cancellationToken = default)
    {
        var entity = _mapper.Map<CylinderSize>(createDto);
        await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<CylinderSizeDto>.Success(_mapper.Map<CylinderSizeDto>(entity));
    }

    public async Task<Result<CylinderSizeDto>> UpdateAsync(Guid id, UpdateCylinderSizeRequest updateDto, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            return Result<CylinderSizeDto>.Failure("Cylinder size not found.");

        _mapper.Map(updateDto, entity);
        _repository.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<CylinderSizeDto>.Success(_mapper.Map<CylinderSizeDto>(entity));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            return Result.Failure("Cylinder size not found.");

        _repository.Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
