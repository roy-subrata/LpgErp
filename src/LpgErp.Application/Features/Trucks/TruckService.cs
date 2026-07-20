using AutoMapper;
using LpgErp.Application.Common.Interfaces;
using LpgErp.Application.Common.Models;
using LpgErp.Application.Features.Trucks.DTOs;
using LpgErp.Domain.Entities;
using LpgErp.Domain.Interfaces;

namespace LpgErp.Application.Features.Trucks;

public interface ITruckService : IService<CreateTruckRequest, UpdateTruckRequest, TruckDto> { }

public class TruckService : ITruckService
{
    private readonly IRepository<Truck> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public TruckService(IRepository<Truck> repository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<TruckDto>>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var allItems = await _repository.GetAllAsync(cancellationToken);
        var totalCount = allItems.Count;
        var items = allItems.OrderBy(x => x.Name).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        return Result<PagedResult<TruckDto>>.Success(new PagedResult<TruckDto>
        {
            Items = _mapper.Map<IReadOnlyList<TruckDto>>(items),
            Pagination = new PaginationMeta { PageNumber = pageNumber, PageSize = pageSize, TotalCount = totalCount }
        });
    }

    public async Task<Result<IReadOnlyList<TruckDto>>> GetAllListAsync(CancellationToken cancellationToken = default)
    {
        var items = await _repository.GetAllAsync(cancellationToken);
        return Result<IReadOnlyList<TruckDto>>.Success(_mapper.Map<IReadOnlyList<TruckDto>>(items));
    }

    public async Task<Result<TruckDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null) return Result<TruckDto>.Failure("Truck not found.");
        return Result<TruckDto>.Success(_mapper.Map<TruckDto>(entity));
    }

    public async Task<Result<TruckDto>> CreateAsync(CreateTruckRequest createDto, CancellationToken cancellationToken = default)
    {
        var entity = _mapper.Map<Truck>(createDto);
        await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<TruckDto>.Success(_mapper.Map<TruckDto>(entity));
    }

    public async Task<Result<TruckDto>> UpdateAsync(Guid id, UpdateTruckRequest updateDto, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null) return Result<TruckDto>.Failure("Truck not found.");
        _mapper.Map(updateDto, entity);
        _repository.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<TruckDto>.Success(_mapper.Map<TruckDto>(entity));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null) return Result.Failure("Truck not found.");
        _repository.Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
