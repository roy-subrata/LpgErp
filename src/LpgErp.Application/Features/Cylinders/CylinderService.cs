using AutoMapper;
using LpgErp.Application.Common.Interfaces;
using LpgErp.Application.Common.Models;
using LpgErp.Application.Features.Cylinders.DTOs;
using LpgErp.Domain.Entities;
using LpgErp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LpgErp.Application.Features.Cylinders;

public interface ICylinderService : IService<CreateCylinderRequest, UpdateCylinderRequest, CylinderDto> { }

public class CylinderService : ICylinderService
{
    private readonly IRepository<Cylinder> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IApplicationDbContext _context;

    public CylinderService(IRepository<Cylinder> repository, IUnitOfWork unitOfWork, IMapper mapper, IApplicationDbContext context)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _context = context;
    }

    public async Task<Result<PagedResult<CylinderDto>>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Cylinders
            .Where(c => !c.IsDeleted)
            .Include(c => c.Brand)
            .Include(c => c.CylinderSize)
            .Include(c => c.CurrentWarehouse)
            .OrderBy(c => c.SerialNumber);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var pagedResult = new PagedResult<CylinderDto>
        {
            Items = _mapper.Map<IReadOnlyList<CylinderDto>>(items),
            Pagination = new PaginationMeta
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            }
        };

        return Result<PagedResult<CylinderDto>>.Success(pagedResult);
    }

    public async Task<Result<IReadOnlyList<CylinderDto>>> GetAllListAsync(CancellationToken cancellationToken = default)
    {
        var items = await _context.Cylinders
            .Where(c => !c.IsDeleted)
            .Include(c => c.Brand)
            .Include(c => c.CylinderSize)
            .Include(c => c.CurrentWarehouse)
            .OrderBy(c => c.SerialNumber)
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<CylinderDto>>.Success(_mapper.Map<IReadOnlyList<CylinderDto>>(items));
    }

    public async Task<Result<CylinderDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Cylinders
            .Include(c => c.Brand)
            .Include(c => c.CylinderSize)
            .Include(c => c.CurrentWarehouse)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, cancellationToken);

        if (entity is null)
            return Result<CylinderDto>.Failure("Cylinder not found.");

        return Result<CylinderDto>.Success(_mapper.Map<CylinderDto>(entity));
    }

    public async Task<Result<CylinderDto>> CreateAsync(CreateCylinderRequest createDto, CancellationToken cancellationToken = default)
    {
        var entity = _mapper.Map<Cylinder>(createDto);
        await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<CylinderDto>.Success(_mapper.Map<CylinderDto>(entity));
    }

    public async Task<Result<CylinderDto>> UpdateAsync(Guid id, UpdateCylinderRequest updateDto, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            return Result<CylinderDto>.Failure("Cylinder not found.");

        _mapper.Map(updateDto, entity);
        _repository.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<CylinderDto>.Success(_mapper.Map<CylinderDto>(entity));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            return Result.Failure("Cylinder not found.");

        _repository.Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
