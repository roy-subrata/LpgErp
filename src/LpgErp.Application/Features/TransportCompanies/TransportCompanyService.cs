using AutoMapper;
using LpgErp.Application.Common.Interfaces;
using LpgErp.Application.Common.Models;
using LpgErp.Application.Features.TransportCompanies.DTOs;
using LpgErp.Domain.Entities;
using LpgErp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LpgErp.Application.Features.TransportCompanies;

public interface ITransportCompanyService
{
    Task<Result<PagedResult<TransportCompanyDto>>> GetAllAsync(int pageNumber, int pageSize, CancellationToken ct = default);
    Task<Result<IReadOnlyList<TransportCompanyDto>>> GetAllListAsync(CancellationToken ct = default);
    Task<Result<TransportCompanyDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<TransportCompanyDto>> CreateAsync(CreateTransportCompanyRequest request, CancellationToken ct = default);
    Task<Result<TransportCompanyDto>> UpdateAsync(Guid id, UpdateTransportCompanyRequest request, CancellationToken ct = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken ct = default);
}

public class TransportCompanyService : ITransportCompanyService
{
    private readonly IApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public TransportCompanyService(IApplicationDbContext context, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<TransportCompanyDto>>> GetAllAsync(int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var query = _context.TransportCompanies.Where(t => !t.IsDeleted).OrderBy(t => t.Name);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return Result<PagedResult<TransportCompanyDto>>.Success(new PagedResult<TransportCompanyDto>
        {
            Items = _mapper.Map<IReadOnlyList<TransportCompanyDto>>(items),
            Pagination = new PaginationMeta { PageNumber = pageNumber, PageSize = pageSize, TotalCount = total }
        });
    }

    public async Task<Result<IReadOnlyList<TransportCompanyDto>>> GetAllListAsync(CancellationToken ct = default)
    {
        var items = await _context.TransportCompanies.Where(t => !t.IsDeleted).OrderBy(t => t.Name).ToListAsync(ct);
        return Result<IReadOnlyList<TransportCompanyDto>>.Success(_mapper.Map<IReadOnlyList<TransportCompanyDto>>(items));
    }

    public async Task<Result<TransportCompanyDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _context.TransportCompanies.FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, ct);
        if (entity is null) return Result<TransportCompanyDto>.Failure("Transport company not found.");
        return Result<TransportCompanyDto>.Success(_mapper.Map<TransportCompanyDto>(entity));
    }

    public async Task<Result<TransportCompanyDto>> CreateAsync(CreateTransportCompanyRequest request, CancellationToken ct = default)
    {
        var entity = new TransportCompany
        {
            Name = request.Name,
            ContactPerson = request.ContactPerson,
            Phone = request.Phone,
            Email = request.Email,
            Address = request.Address,
            IsActive = request.IsActive
        };
        await _context.TransportCompanies.AddAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result<TransportCompanyDto>.Success(_mapper.Map<TransportCompanyDto>(entity));
    }

    public async Task<Result<TransportCompanyDto>> UpdateAsync(Guid id, UpdateTransportCompanyRequest request, CancellationToken ct = default)
    {
        var entity = await _context.TransportCompanies.FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, ct);
        if (entity is null) return Result<TransportCompanyDto>.Failure("Transport company not found.");
        entity.Name = request.Name;
        entity.ContactPerson = request.ContactPerson;
        entity.Phone = request.Phone;
        entity.Email = request.Email;
        entity.Address = request.Address;
        entity.IsActive = request.IsActive;
        await _unitOfWork.SaveChangesAsync(ct);
        return Result<TransportCompanyDto>.Success(_mapper.Map<TransportCompanyDto>(entity));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _context.TransportCompanies.FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, ct);
        if (entity is null) return Result.Failure("Transport company not found.");
        _context.TransportCompanies.Remove(entity);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
