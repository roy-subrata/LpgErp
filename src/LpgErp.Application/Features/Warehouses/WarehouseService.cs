using AutoMapper;
using LpgErp.Application.Common.Interfaces;
using LpgErp.Application.Common.Models;
using LpgErp.Application.Features.Warehouses.DTOs;
using LpgErp.Domain.Entities;
using LpgErp.Domain.Interfaces;

namespace LpgErp.Application.Features.Warehouses;

public interface IWarehouseService : IService<CreateWarehouseRequest, UpdateWarehouseRequest, WarehouseDto> { }

public class WarehouseService : IWarehouseService
{
    private readonly IRepository<Warehouse> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public WarehouseService(IRepository<Warehouse> repository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<WarehouseDto>>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var allItems = await _repository.GetAllAsync(cancellationToken);
        var totalCount = allItems.Count;
        var items = allItems
            .OrderBy(x => x.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var pagedResult = new PagedResult<WarehouseDto>
        {
            Items = _mapper.Map<IReadOnlyList<WarehouseDto>>(items),
            Pagination = new PaginationMeta
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            }
        };

        return Result<PagedResult<WarehouseDto>>.Success(pagedResult);
    }

    public async Task<Result<IReadOnlyList<WarehouseDto>>> GetAllListAsync(CancellationToken cancellationToken = default)
    {
        var items = await _repository.GetAllAsync(cancellationToken);
        return Result<IReadOnlyList<WarehouseDto>>.Success(_mapper.Map<IReadOnlyList<WarehouseDto>>(items));
    }

    public async Task<Result<WarehouseDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            return Result<WarehouseDto>.Failure("Warehouse not found.");

        return Result<WarehouseDto>.Success(_mapper.Map<WarehouseDto>(entity));
    }

    public async Task<Result<WarehouseDto>> CreateAsync(CreateWarehouseRequest createDto, CancellationToken cancellationToken = default)
    {
        var entity = _mapper.Map<Warehouse>(createDto);
        await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<WarehouseDto>.Success(_mapper.Map<WarehouseDto>(entity));
    }

    public async Task<Result<WarehouseDto>> UpdateAsync(Guid id, UpdateWarehouseRequest updateDto, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            return Result<WarehouseDto>.Failure("Warehouse not found.");

        _mapper.Map(updateDto, entity);
        _repository.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<WarehouseDto>.Success(_mapper.Map<WarehouseDto>(entity));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            return Result.Failure("Warehouse not found.");

        _repository.Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
