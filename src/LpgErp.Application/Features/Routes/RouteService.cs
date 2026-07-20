using AutoMapper;
using LpgErp.Application.Common.Interfaces;
using LpgErp.Application.Common.Models;
using LpgErp.Application.Features.Routes.DTOs;
using LpgErp.Domain.Entities;
using LpgErp.Domain.Interfaces;

namespace LpgErp.Application.Features.Routes;

public interface IRouteService : IService<CreateRouteRequest, UpdateRouteRequest, RouteDto> { }

public class RouteService : IRouteService
{
    private readonly IRepository<Route> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public RouteService(IRepository<Route> repository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<RouteDto>>> GetAllAsync(int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var all = await _repository.GetAllAsync(ct);
        var total = all.Count;
        var items = all.OrderBy(x => x.Name).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        return Result<PagedResult<RouteDto>>.Success(new PagedResult<RouteDto>
        {
            Items = _mapper.Map<IReadOnlyList<RouteDto>>(items),
            Pagination = new PaginationMeta { PageNumber = pageNumber, PageSize = pageSize, TotalCount = total }
        });
    }

    public async Task<Result<IReadOnlyList<RouteDto>>> GetAllListAsync(CancellationToken ct = default)
    {
        var items = await _repository.GetAllAsync(ct);
        return Result<IReadOnlyList<RouteDto>>.Success(_mapper.Map<IReadOnlyList<RouteDto>>(items));
    }

    public async Task<Result<RouteDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null) return Result<RouteDto>.Failure("Route not found.");
        return Result<RouteDto>.Success(_mapper.Map<RouteDto>(entity));
    }

    public async Task<Result<RouteDto>> CreateAsync(CreateRouteRequest req, CancellationToken ct = default)
    {
        var entity = _mapper.Map<Route>(req);
        await _repository.AddAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result<RouteDto>.Success(_mapper.Map<RouteDto>(entity));
    }

    public async Task<Result<RouteDto>> UpdateAsync(Guid id, UpdateRouteRequest req, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null) return Result<RouteDto>.Failure("Route not found.");
        _mapper.Map(req, entity);
        _repository.Update(entity);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result<RouteDto>.Success(_mapper.Map<RouteDto>(entity));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null) return Result.Failure("Route not found.");
        _repository.Delete(entity);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
