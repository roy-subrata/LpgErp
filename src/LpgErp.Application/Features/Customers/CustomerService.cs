using AutoMapper;
using LpgErp.Application.Common.Interfaces;
using LpgErp.Application.Common.Models;
using LpgErp.Application.Features.Customers.DTOs;
using LpgErp.Domain.Entities;
using LpgErp.Domain.Interfaces;

namespace LpgErp.Application.Features.Customers;

public interface ICustomerService : IService<CreateCustomerRequest, UpdateCustomerRequest, CustomerDto> { }

public class CustomerService : ICustomerService
{
    private readonly IRepository<Customer> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CustomerService(IRepository<Customer> repository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<CustomerDto>>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var allItems = await _repository.GetAllAsync(cancellationToken);
        var totalCount = allItems.Count;
        var items = allItems
            .OrderBy(x => x.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var pagedResult = new PagedResult<CustomerDto>
        {
            Items = _mapper.Map<IReadOnlyList<CustomerDto>>(items),
            Pagination = new PaginationMeta
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            }
        };

        return Result<PagedResult<CustomerDto>>.Success(pagedResult);
    }

    public async Task<Result<IReadOnlyList<CustomerDto>>> GetAllListAsync(CancellationToken cancellationToken = default)
    {
        var items = await _repository.GetAllAsync(cancellationToken);
        return Result<IReadOnlyList<CustomerDto>>.Success(_mapper.Map<IReadOnlyList<CustomerDto>>(items));
    }

    public async Task<Result<CustomerDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            return Result<CustomerDto>.Failure("Customer not found.");

        return Result<CustomerDto>.Success(_mapper.Map<CustomerDto>(entity));
    }

    public async Task<Result<CustomerDto>> CreateAsync(CreateCustomerRequest createDto, CancellationToken cancellationToken = default)
    {
        var entity = _mapper.Map<Customer>(createDto);
        await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<CustomerDto>.Success(_mapper.Map<CustomerDto>(entity));
    }

    public async Task<Result<CustomerDto>> UpdateAsync(Guid id, UpdateCustomerRequest updateDto, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            return Result<CustomerDto>.Failure("Customer not found.");

        _mapper.Map(updateDto, entity);
        _repository.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<CustomerDto>.Success(_mapper.Map<CustomerDto>(entity));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            return Result.Failure("Customer not found.");

        _repository.Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
