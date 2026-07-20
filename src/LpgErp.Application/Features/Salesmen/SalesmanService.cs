using AutoMapper;
using LpgErp.Application.Common.Interfaces;
using LpgErp.Application.Common.Models;
using LpgErp.Application.Features.Salesmen.DTOs;
using LpgErp.Domain.Entities;
using LpgErp.Domain.Interfaces;

namespace LpgErp.Application.Features.Salesmen;

public interface ISalesmanService : IService<CreateSalesmanRequest, UpdateSalesmanRequest, SalesmanDto> { }

public class SalesmanService : ISalesmanService
{
    private readonly IRepository<Salesman> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SalesmanService(IRepository<Salesman> repository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<SalesmanDto>>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var allItems = await _repository.GetAllAsync(cancellationToken);
        var totalCount = allItems.Count;
        var items = allItems.OrderBy(x => x.Name).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        return Result<PagedResult<SalesmanDto>>.Success(new PagedResult<SalesmanDto>
        {
            Items = _mapper.Map<IReadOnlyList<SalesmanDto>>(items),
            Pagination = new PaginationMeta { PageNumber = pageNumber, PageSize = pageSize, TotalCount = totalCount }
        });
    }

    public async Task<Result<IReadOnlyList<SalesmanDto>>> GetAllListAsync(CancellationToken cancellationToken = default)
    {
        var items = await _repository.GetAllAsync(cancellationToken);
        return Result<IReadOnlyList<SalesmanDto>>.Success(_mapper.Map<IReadOnlyList<SalesmanDto>>(items));
    }

    public async Task<Result<SalesmanDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null) return Result<SalesmanDto>.Failure("Salesman not found.");
        return Result<SalesmanDto>.Success(_mapper.Map<SalesmanDto>(entity));
    }

    public async Task<Result<SalesmanDto>> CreateAsync(CreateSalesmanRequest createDto, CancellationToken cancellationToken = default)
    {
        var entity = _mapper.Map<Salesman>(createDto);
        await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<SalesmanDto>.Success(_mapper.Map<SalesmanDto>(entity));
    }

    public async Task<Result<SalesmanDto>> UpdateAsync(Guid id, UpdateSalesmanRequest updateDto, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null) return Result<SalesmanDto>.Failure("Salesman not found.");
        _mapper.Map(updateDto, entity);
        _repository.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<SalesmanDto>.Success(_mapper.Map<SalesmanDto>(entity));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null) return Result.Failure("Salesman not found.");
        _repository.Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
