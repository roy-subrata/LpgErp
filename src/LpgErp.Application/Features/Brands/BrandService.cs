using AutoMapper;
using LpgErp.Application.Common.Interfaces;
using LpgErp.Application.Common.Models;
using LpgErp.Application.Features.Brands.DTOs;
using LpgErp.Domain.Entities;
using LpgErp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LpgErp.Application.Features.Brands;

public interface IBrandService : IService<CreateBrandRequest, UpdateBrandRequest, BrandDto> { }

public class BrandService : IBrandService
{
    private readonly IRepository<Brand> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public BrandService(IRepository<Brand> repository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<BrandDto>>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var allItems = await _repository.GetAllAsync(cancellationToken);
        var totalCount = allItems.Count;
        var items = allItems
            .OrderBy(x => x.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var pagedResult = new PagedResult<BrandDto>
        {
            Items = _mapper.Map<IReadOnlyList<BrandDto>>(items),
            Pagination = new PaginationMeta
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            }
        };

        return Result<PagedResult<BrandDto>>.Success(pagedResult);
    }

    public async Task<Result<IReadOnlyList<BrandDto>>> GetAllListAsync(CancellationToken cancellationToken = default)
    {
        var items = await _repository.GetAllAsync(cancellationToken);
        return Result<IReadOnlyList<BrandDto>>.Success(_mapper.Map<IReadOnlyList<BrandDto>>(items));
    }

    public async Task<Result<BrandDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            return Result<BrandDto>.Failure("Brand not found.");

        return Result<BrandDto>.Success(_mapper.Map<BrandDto>(entity));
    }

    public async Task<Result<BrandDto>> CreateAsync(CreateBrandRequest createDto, CancellationToken cancellationToken = default)
    {
        var entity = _mapper.Map<Brand>(createDto);
        await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<BrandDto>.Success(_mapper.Map<BrandDto>(entity));
    }

    public async Task<Result<BrandDto>> UpdateAsync(Guid id, UpdateBrandRequest updateDto, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            return Result<BrandDto>.Failure("Brand not found.");

        _mapper.Map(updateDto, entity);
        _repository.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<BrandDto>.Success(_mapper.Map<BrandDto>(entity));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            return Result.Failure("Brand not found.");

        _repository.Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
