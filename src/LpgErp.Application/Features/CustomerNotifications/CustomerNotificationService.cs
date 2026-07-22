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
    Task<Result<int>> GenerateAsync(CancellationToken ct = default);
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

    /// <summary>
    /// Scans customers and creates the business-rule notifications (payment due, credit limit exceeded,
    /// outstanding empty cylinders, return reminders, possible refill time). Idempotent: a customer never
    /// gets a second UNREAD notification of the same type.
    /// </summary>
    public async Task<Result<int>> GenerateAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var customers = await _context.Customers.Where(c => !c.IsDeleted && c.IsActive).ToListAsync(ct);

        // Outstanding receivable per customer: delivered credit orders minus inbound payments on them.
        var orderTotals = await _context.SalesOrders
            .Where(o => !o.IsDeleted && o.Status == SalesOrderStatus.Delivered && o.IsCreditSale)
            .GroupBy(o => o.CustomerId)
            .Select(g => new { g.Key, Total = g.Sum(o => o.TotalAmount - o.Discount), MinDueDate = g.Min(o => o.DueDate) })
            .ToDictionaryAsync(x => x.Key, x => new { x.Total, x.MinDueDate }, ct);

        var paymentTotals = await _context.Payments
            .Where(p => !p.IsDeleted && p.Direction == PaymentDirection.Inbound && p.SalesOrderId != null)
            .GroupBy(p => p.SalesOrder!.CustomerId)
            .Select(g => new { g.Key, Total = g.Sum(p => p.Amount) })
            .ToDictionaryAsync(x => x.Key, x => x.Total, ct);

        var cylinderOutstanding = await _context.CustomerCylinderBalances
            .Where(b => !b.IsDeleted)
            .GroupBy(b => b.CustomerId)
            .Select(g => new { g.Key, Outstanding = g.Sum(b => b.Received - b.Returned), LastChange = g.Max(b => b.UpdatedAt ?? b.CreatedAt) })
            .ToDictionaryAsync(x => x.Key, x => new { x.Outstanding, x.LastChange }, ct);

        var lastRefillDates = await _context.SalesOrderItems
            .Where(i => !i.IsDeleted && !i.SalesOrder.IsDeleted && i.SalesOrder.Status == SalesOrderStatus.Delivered
                && (i.Product.Type == ProductType.GasRefill || i.Product.Type == ProductType.NewPackage))
            .GroupBy(i => i.SalesOrder.CustomerId)
            .Select(g => new { g.Key, Last = g.Max(i => i.SalesOrder.OrderDate) })
            .ToDictionaryAsync(x => x.Key, x => x.Last, ct);

        // Existing unread notifications: (customer, type) pairs to skip.
        var existingUnread = (await _context.CustomerNotifications
            .Where(n => !n.IsDeleted && !n.IsRead)
            .Select(n => new { n.CustomerId, n.Type })
            .ToListAsync(ct))
            .Select(x => (x.CustomerId, x.Type))
            .ToHashSet();

        var created = 0;
        void Notify(Guid customerId, NotificationType type, string title, string message)
        {
            if (existingUnread.Contains((customerId, type))) return;
            _context.CustomerNotifications.Add(new CustomerNotification
            {
                CustomerId = customerId,
                Type = type,
                Title = title,
                Message = message
            });
            existingUnread.Add((customerId, type));
            created++;
        }

        foreach (var customer in customers)
        {
            var outstanding = orderTotals.GetValueOrDefault(customer.Id)?.Total ?? 0m;
            outstanding -= paymentTotals.GetValueOrDefault(customer.Id);

            if (outstanding > 0)
            {
                var dueDate = orderTotals.GetValueOrDefault(customer.Id)?.MinDueDate;
                if (dueDate is not null && dueDate <= now.AddDays(3))
                    Notify(customer.Id, NotificationType.PaymentDue, "Payment due",
                        $"Outstanding balance of ৳{outstanding:N0} — earliest due date {dueDate:dd MMM yyyy}.");

                if (customer.CreditLimit > 0 && outstanding > customer.CreditLimit)
                    Notify(customer.Id, NotificationType.CreditLimitExceeded, "Credit limit exceeded",
                        $"Outstanding ৳{outstanding:N0} exceeds the credit limit of ৳{customer.CreditLimit:N0}.");
            }

            var cyl = cylinderOutstanding.GetValueOrDefault(customer.Id);
            if (cyl is not null && cyl.Outstanding > 0)
            {
                Notify(customer.Id, NotificationType.OutstandingEmptyCylinder, "Outstanding empty cylinders",
                    $"{cyl.Outstanding} empty cylinder(s) not yet returned.");

                if (cyl.LastChange <= now.AddDays(-7))
                    Notify(customer.Id, NotificationType.CylinderReturnReminder, "Cylinder return reminder",
                        $"{cyl.Outstanding} cylinder(s) outstanding for more than a week — please arrange return.");
            }

            if (lastRefillDates.TryGetValue(customer.Id, out var lastRefill) && lastRefill <= now.AddDays(-21))
                Notify(customer.Id, NotificationType.PossibleRefillTime, "Possible refill time",
                    $"Last refill was on {lastRefill:dd MMM yyyy} — the customer is likely due for a refill.");
        }

        if (created > 0) await _unitOfWork.SaveChangesAsync(ct);
        return Result<int>.Success(created);
    }
}
