using AutoMapper;
using LpgErp.Application.Common.Interfaces;
using LpgErp.Application.Common.Models;
using LpgErp.Application.Features.Drivers.DTOs;
using LpgErp.Domain.Entities;
using LpgErp.Domain.Interfaces;

namespace LpgErp.Application.Features.Drivers;

public interface IDriverService : IService<CreateDriverRequest, UpdateDriverRequest, DriverDto> { }

public class DriverService : IDriverService
{
    private readonly IRepository<Driver> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public DriverService(IRepository<Driver> repository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<DriverDto>>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var allItems = await _repository.GetAllAsync(cancellationToken);
        var totalCount = allItems.Count;
        var items = allItems.OrderBy(x => x.Name).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        return Result<PagedResult<DriverDto>>.Success(new PagedResult<DriverDto>
        {
            Items = _mapper.Map<IReadOnlyList<DriverDto>>(items),
            Pagination = new PaginationMeta { PageNumber = pageNumber, PageSize = pageSize, TotalCount = totalCount }
        });
    }

    public async Task<Result<IReadOnlyList<DriverDto>>> GetAllListAsync(CancellationToken cancellationToken = default)
    {
        var items = await _repository.GetAllAsync(cancellationToken);
        return Result<IReadOnlyList<DriverDto>>.Success(_mapper.Map<IReadOnlyList<DriverDto>>(items));
    }

    public async Task<Result<DriverDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null) return Result<DriverDto>.Failure("Driver not found.");
        return Result<DriverDto>.Success(_mapper.Map<DriverDto>(entity));
    }

    public async Task<Result<DriverDto>> CreateAsync(CreateDriverRequest createDto, CancellationToken cancellationToken = default)
    {
        var entity = _mapper.Map<Driver>(createDto);
        await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<DriverDto>.Success(_mapper.Map<DriverDto>(entity));
    }

    public async Task<Result<DriverDto>> UpdateAsync(Guid id, UpdateDriverRequest updateDto, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null) return Result<DriverDto>.Failure("Driver not found.");
        _mapper.Map(updateDto, entity);
        _repository.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<DriverDto>.Success(_mapper.Map<DriverDto>(entity));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null) return Result.Failure("Driver not found.");
        _repository.Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
