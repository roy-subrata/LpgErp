using AutoMapper;
using LpgErp.Application.Common.Interfaces;
using LpgErp.Application.Common.Models;
using LpgErp.Application.Features.CustomerNotifications.DTOs;
using LpgErp.Domain.Entities;
using LpgErp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LpgErp.Application.Features.CustomerNotifications;

public interface ICustomerNotificationService
{
    Task<Result<PagedResult<CustomerNotificationDto>>> GetAllAsync(int pageNumber, int pageSize, CancellationToken ct = default);
    Task<Result<IReadOnlyList<CustomerNotificationDto>>> GetByCustomerAsync(Guid customerId, CancellationToken ct = default);
    Task<Result<CustomerNotificationDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<CustomerNotificationDto>> CreateAsync(CreateCustomerNotificationRequest request, CancellationToken ct = default);
    Task<Result<CustomerNotificationDto>> UpdateAsync(Guid id, UpdateCustomerNotificationRequest request, CancellationToken ct = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<Result> MarkAsReadAsync(Guid id, CancellationToken ct = default);
}

public class CustomerNotificationService : ICustomerNotificationService
{
    private readonly IApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CustomerNotificationService(IApplicationDbContext context, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<CustomerNotificationDto>>> GetAllAsync(int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var query = _context.CustomerNotifications.Where(n => !n.IsDeleted).Include(n => n.Customer).OrderByDescending(n => n.CreatedAt);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return Result<PagedResult<CustomerNotificationDto>>.Success(new PagedResult<CustomerNotificationDto>
        {
            Items = _mapper.Map<IReadOnlyList<CustomerNotificationDto>>(items),
            Pagination = new PaginationMeta { PageNumber = pageNumber, PageSize = pageSize, TotalCount = total }
        });
    }

    public async Task<Result<IReadOnlyList<CustomerNotificationDto>>> GetByCustomerAsync(Guid customerId, CancellationToken ct = default)
    {
        var items = await _context.CustomerNotifications
            .Where(n => n.CustomerId == customerId && !n.IsDeleted)
            .Include(n => n.Customer)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(ct);
        return Result<IReadOnlyList<CustomerNotificationDto>>.Success(_mapper.Map<IReadOnlyList<CustomerNotificationDto>>(items));
    }

    public async Task<Result<CustomerNotificationDto>> CreateAsync(CreateCustomerNotificationRequest request, CancellationToken ct = default)
    {
        var entity = _mapper.Map<CustomerNotification>(request);
        await _context.CustomerNotifications.AddAsync(entity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var result = await _context.CustomerNotifications.Include(n => n.Customer).FirstOrDefaultAsync(n => n.Id == entity.Id, ct);
        return Result<CustomerNotificationDto>.Success(_mapper.Map<CustomerNotificationDto>(result));
    }

    public async Task<Result<CustomerNotificationDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _context.CustomerNotifications.Include(n => n.Customer).FirstOrDefaultAsync(n => n.Id == id && !n.IsDeleted, ct);
        if (entity is null) return Result<CustomerNotificationDto>.Failure("Customer notification not found.");
        return Result<CustomerNotificationDto>.Success(_mapper.Map<CustomerNotificationDto>(entity));
    }

    public async Task<Result<CustomerNotificationDto>> UpdateAsync(Guid id, UpdateCustomerNotificationRequest request, CancellationToken ct = default)
    {
        var entity = await _context.CustomerNotifications.FindAsync([id], ct);
        if (entity is null || entity.IsDeleted) return Result<CustomerNotificationDto>.Failure("Customer notification not found.");

        _mapper.Map(request, entity);
        await _unitOfWork.SaveChangesAsync(ct);

        var result = await _context.CustomerNotifications.Include(n => n.Customer).FirstOrDefaultAsync(n => n.Id == entity.Id, ct);
        return Result<CustomerNotificationDto>.Success(_mapper.Map<CustomerNotificationDto>(result));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _context.CustomerNotifications.FindAsync([id], ct);
        if (entity is null || entity.IsDeleted) return Result.Failure("Customer notification not found.");

        _context.CustomerNotifications.Remove(entity);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> MarkAsReadAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _context.CustomerNotifications.FindAsync([id], ct);
        if (entity is null) return Result.Failure("Notification not found.");
        entity.IsRead = true;
        entity.ReadAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
