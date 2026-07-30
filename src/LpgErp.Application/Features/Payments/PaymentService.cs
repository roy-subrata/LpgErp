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
    private readonly INotificationService _notificationService;

    public PaymentService(IApplicationDbContext context, IUnitOfWork unitOfWork, IMapper mapper, INotificationService notificationService)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _notificationService = notificationService;
    }

    public async Task<Result<PagedResult<PaymentDto>>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Payments
            .Where(p => !p.IsDeleted)
            .Include(p => p.SalesOrder)
            .Include(p => p.PurchaseOrder)
            .Include(p => p.PaymentAccount)
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
            .Include(p => p.PaymentAccount)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, cancellationToken);

        if (entity is null) return Result<PaymentDto>.Failure("Payment not found.");
        return Result<PaymentDto>.Success(_mapper.Map<PaymentDto>(entity));
    }

    public async Task<Result<PaymentDto>> CreateAsync(CreatePaymentRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Amount <= 0) return Result<PaymentDto>.Failure("Payment amount must be greater than zero.");

        if (request.Direction == PaymentDirection.Inbound && request.SalesOrderId is Guid soId)
        {
            var order = await _context.SalesOrders.FindAsync([soId], cancellationToken);
            if (order is null) return Result<PaymentDto>.Failure("Sales order not found.");

            var totalPaid = await _context.Payments
                .Where(p => !p.IsDeleted && p.SalesOrderId == soId && p.Direction == PaymentDirection.Inbound)
                .SumAsync(p => p.Amount, cancellationToken);
            if (totalPaid + request.Amount > order.NetAmount)
                return Result<PaymentDto>.Failure($"Payment ({request.Amount:N2}) exceeds outstanding balance ({order.NetAmount - totalPaid:N2}).");
        }

        if (request.Direction == PaymentDirection.Outbound && request.PurchaseOrderId is Guid poId)
        {
            var order = await _context.PurchaseOrders.FindAsync([poId], cancellationToken);
            if (order is null) return Result<PaymentDto>.Failure("Purchase order not found.");

            var totalPaid = await _context.Payments
                .Where(p => !p.IsDeleted && p.PurchaseOrderId == poId && p.Direction == PaymentDirection.Outbound)
                .SumAsync(p => p.Amount, cancellationToken);
            if (totalPaid + request.Amount > order.NetPayable)
                return Result<PaymentDto>.Failure($"Payment ({request.Amount:N2}) exceeds outstanding balance.");
        }

        if (request.Direction == PaymentDirection.Inbound && request.PurchaseOrderId is not null)
            return Result<PaymentDto>.Failure("Inbound payments cannot be linked to a purchase order.");
        if (request.Direction == PaymentDirection.Outbound && request.SalesOrderId is not null)
            return Result<PaymentDto>.Failure("Outbound payments cannot be linked to a sales order.");

        if (await ValidateAccountAsync(request.PaymentAccountId, request.Method, cancellationToken) is string accountError)
            return Result<PaymentDto>.Failure(accountError);

        var entity = _mapper.Map<Payment>(request);
        await _context.Payments.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (request.Direction == PaymentDirection.Inbound && request.SalesOrderId.HasValue)
        {
            try
            {
                var order = await _context.SalesOrders.FindAsync([request.SalesOrderId.Value], cancellationToken);
                if (order != null)
                {
                    await _notificationService.NotifyPaymentReceivedAsync(
                        order.OrderNumber,
                        request.Amount,
                        request.Method.ToString(),
                        order.CustomerId);
                }
            }
            catch { /* notification failure should not break the main operation */ }
        }
        else if (request.Direction == PaymentDirection.Outbound && request.PurchaseOrderId.HasValue)
        {
            try
            {
                var order = await _context.PurchaseOrders.FindAsync([request.PurchaseOrderId.Value], cancellationToken);
                if (order != null)
                {
                    await _notificationService.NotifyPaymentOutboundAsync(
                        order.OrderNumber,
                        request.Amount,
                        request.Method.ToString());
                }
            }
            catch { /* notification failure should not break the main operation */ }
        }

        return await GetByIdAsync(entity.Id, cancellationToken);
    }

    public async Task<Result<PaymentDto>> UpdateAsync(Guid id, UpdatePaymentRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Payments.FindAsync([id], cancellationToken);
        if (entity is null) return Result<PaymentDto>.Failure("Payment not found.");

        if (request.Amount <= 0) return Result<PaymentDto>.Failure("Payment amount must be greater than zero.");
        if (request.Direction == PaymentDirection.Inbound && request.PurchaseOrderId is not null)
            return Result<PaymentDto>.Failure("Inbound payments cannot be linked to a purchase order.");
        if (request.Direction == PaymentDirection.Outbound && request.SalesOrderId is not null)
            return Result<PaymentDto>.Failure("Outbound payments cannot be linked to a sales order.");

        if (request.Direction == PaymentDirection.Inbound && request.SalesOrderId is Guid soId)
        {
            var order = await _context.SalesOrders.FindAsync([soId], cancellationToken);
            if (order is null) return Result<PaymentDto>.Failure("Sales order not found.");

            var totalPaid = await _context.Payments
                .Where(p => !p.IsDeleted && p.SalesOrderId == soId && p.Direction == PaymentDirection.Inbound && p.Id != id)
                .SumAsync(p => p.Amount, cancellationToken);
            if (totalPaid + request.Amount > order.NetAmount)
                return Result<PaymentDto>.Failure($"Payment ({request.Amount:N2}) exceeds outstanding balance ({order.NetAmount - totalPaid:N2}).");
        }

        if (request.Direction == PaymentDirection.Outbound && request.PurchaseOrderId is Guid poId)
        {
            var order = await _context.PurchaseOrders.FindAsync([poId], cancellationToken);
            if (order is null) return Result<PaymentDto>.Failure("Purchase order not found.");

            var totalPaid = await _context.Payments
                .Where(p => !p.IsDeleted && p.PurchaseOrderId == poId && p.Direction == PaymentDirection.Outbound && p.Id != id)
                .SumAsync(p => p.Amount, cancellationToken);
            if (totalPaid + request.Amount > order.NetPayable)
                return Result<PaymentDto>.Failure($"Payment ({request.Amount:N2}) exceeds outstanding balance.");
        }

        if (await ValidateAccountAsync(request.PaymentAccountId, request.Method, cancellationToken) is string accountError)
            return Result<PaymentDto>.Failure(accountError);

        entity.Amount = request.Amount;
        entity.PaymentDate = request.PaymentDate;
        entity.Method = request.Method;
        entity.PaymentAccountId = request.PaymentAccountId;
        entity.Direction = request.Direction;
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

    private Task<string?> ValidateAccountAsync(Guid? accountId, PaymentMethod method, CancellationToken cancellationToken) =>
        PaymentAccountRules.ValidateAsync(_context, accountId, method, cancellationToken);
}
