using AutoMapper;
using LpgErp.Application.Common.Interfaces;
using LpgErp.Application.Common.Models;
using LpgErp.Application.Features.Payments.DTOs;
using LpgErp.Domain.Entities;
using LpgErp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LpgErp.Application.Features.Payments;

public interface IPaymentService
{
    Task<Result<PagedResult<PaymentDto>>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<Result<PaymentDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<PaymentDto>> CreateAsync(CreatePaymentRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<PaymentDto>> UpdateAsync(Guid id, UpdatePaymentRequest request, CancellationToken cancellationToken = default);
}

public class PaymentService : IPaymentService
{
    private readonly IApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public PaymentService(IApplicationDbContext context, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<PaymentDto>>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Payments
            .Where(p => !p.IsDeleted)
            .Include(p => p.SalesOrder)
            .Include(p => p.PurchaseOrder)
            .OrderByDescending(p => p.PaymentDate);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return Result<PagedResult<PaymentDto>>.Success(new PagedResult<PaymentDto>
        {
            Items = _mapper.Map<IReadOnlyList<PaymentDto>>(items),
            Pagination = new PaginationMeta { PageNumber = pageNumber, PageSize = pageSize, TotalCount = totalCount }
        });
    }

    public async Task<Result<PaymentDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Payments
            .Include(p => p.SalesOrder)
            .Include(p => p.PurchaseOrder)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, cancellationToken);

        if (entity is null) return Result<PaymentDto>.Failure("Payment not found.");
        return Result<PaymentDto>.Success(_mapper.Map<PaymentDto>(entity));
    }

    public async Task<Result<PaymentDto>> CreateAsync(CreatePaymentRequest request, CancellationToken cancellationToken = default)
    {
        var entity = _mapper.Map<Payment>(request);
        await _context.Payments.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(entity.Id, cancellationToken);
    }

    public async Task<Result<PaymentDto>> UpdateAsync(Guid id, UpdatePaymentRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Payments.FindAsync([id], cancellationToken);
        if (entity is null) return Result<PaymentDto>.Failure("Payment not found.");

        entity.Amount = request.Amount;
        entity.PaymentDate = request.PaymentDate;
        entity.Method = request.Method;
        entity.Reference = request.Reference;
        entity.Notes = request.Notes;
        entity.SalesOrderId = request.SalesOrderId;
        entity.PurchaseOrderId = request.PurchaseOrderId;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Payments.FindAsync([id], cancellationToken);
        if (entity is null) return Result.Failure("Payment not found.");

        _context.Payments.Remove(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
