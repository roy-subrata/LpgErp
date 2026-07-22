using AutoMapper;
using LpgErp.Application.Common.Interfaces;
using LpgErp.Application.Common.Mappings;
using LpgErp.Application.Common.Models;
using LpgErp.Domain.Entities;
using LpgErp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LpgErp.Application.Features.CustomerCylinderLedger;

public interface ICustomerCylinderLedgerService
{
    Task<Result<IReadOnlyList<CustomerCylinderBalanceDto>>> GetByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<Result<PagedResult<CustomerCylinderBalanceDto>>> GetAllPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<Result<CustomerCylinderBalanceDto>> AdjustBalanceAsync(AdjustCylinderBalanceRequest request, CancellationToken cancellationToken = default);
}

public class CustomerCylinderLedgerService : ICustomerCylinderLedgerService
{
    private readonly IApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CustomerCylinderLedgerService(IApplicationDbContext context, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<IReadOnlyList<CustomerCylinderBalanceDto>>> GetByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var items = await _context.CustomerCylinderBalances
            .Where(c => c.CustomerId == customerId && !c.IsDeleted)
            .Include(c => c.Customer)
            .Include(c => c.Brand)
            .Include(c => c.CylinderSize)
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<CustomerCylinderBalanceDto>>.Success(
            _mapper.Map<IReadOnlyList<CustomerCylinderBalanceDto>>(items));
    }

    public async Task<Result<PagedResult<CustomerCylinderBalanceDto>>> GetAllPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.CustomerCylinderBalances
            .Where(c => !c.IsDeleted)
            .Include(c => c.Customer)
            .Include(c => c.Brand)
            .Include(c => c.CylinderSize)
            .OrderBy(c => c.Customer.Name);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return Result<PagedResult<CustomerCylinderBalanceDto>>.Success(new PagedResult<CustomerCylinderBalanceDto>
        {
            Items = _mapper.Map<IReadOnlyList<CustomerCylinderBalanceDto>>(items),
            Pagination = new PaginationMeta { PageNumber = pageNumber, PageSize = pageSize, TotalCount = totalCount }
        });
    }

    public async Task<Result<CustomerCylinderBalanceDto>> AdjustBalanceAsync(AdjustCylinderBalanceRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Quantity <= 0)
            return Result<CustomerCylinderBalanceDto>.Failure("Quantity must be positive.");

        var balance = await _context.CustomerCylinderBalances
            .Include(c => c.Customer)
            .Include(c => c.Brand)
            .Include(c => c.CylinderSize)
            .FirstOrDefaultAsync(c => c.CustomerId == request.CustomerId
                && c.BrandId == request.BrandId
                && c.CylinderSizeId == request.CylinderSizeId
                && !c.IsDeleted, cancellationToken);

        if (request.IsReturn && request.Quantity > (balance is null ? 0 : balance.Received - balance.Returned))
            return Result<CustomerCylinderBalanceDto>.Failure("Cannot return more cylinders than are outstanding.");

        // Optional monetary settlement: the returned cylinders reduce a credit sales order's due.
        Payment? settlement = null;
        if (request.SettlementAmount > 0)
        {
            if (!request.IsReturn)
                return Result<CustomerCylinderBalanceDto>.Failure("A credit settlement applies only to cylinder returns.");
            if (request.SettlementSalesOrderId is null)
                return Result<CustomerCylinderBalanceDto>.Failure("Select the credit sales order to settle against.");

            var order = await _context.SalesOrders
                .FirstOrDefaultAsync(o => o.Id == request.SettlementSalesOrderId && !o.IsDeleted, cancellationToken);
            if (order is null || order.CustomerId != request.CustomerId)
                return Result<CustomerCylinderBalanceDto>.Failure("Sales order not found for this customer.");
            if (order.Status != SalesOrderStatus.Delivered || !order.IsCreditSale)
                return Result<CustomerCylinderBalanceDto>.Failure("Settlements apply to delivered credit sales orders.");

            var paid = await _context.Payments
                .Where(p => !p.IsDeleted && p.SalesOrderId == order.Id && p.Direction == PaymentDirection.Inbound)
                .SumAsync(p => p.Amount, cancellationToken);
            var due = order.TotalAmount - order.Discount - paid;
            if (request.SettlementAmount > due)
                return Result<CustomerCylinderBalanceDto>.Failure($"Settlement exceeds the order's remaining due of ৳{due:N0}.");

            settlement = new Payment
            {
                SalesOrderId = order.Id,
                Direction = PaymentDirection.Inbound,
                Method = PaymentMethod.Cash,
                Amount = request.SettlementAmount,
                PaymentDate = DateTime.UtcNow,
                Reference = "CYL-RETURN",
                Notes = $"Credit adjusted for {request.Quantity} returned cylinder(s)."
            };
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            if (balance is null)
            {
                balance = new CustomerCylinderBalance
                {
                    CustomerId = request.CustomerId,
                    BrandId = request.BrandId,
                    CylinderSizeId = request.CylinderSizeId,
                    Received = 0,
                    Returned = 0
                };
                await _context.CustomerCylinderBalances.AddAsync(balance, cancellationToken);
            }

            if (request.IsReturn)
                balance.Returned += request.Quantity;
            else
                balance.Received += request.Quantity;

            if (settlement is not null)
                await _context.Payments.AddAsync(settlement, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }

        return Result<CustomerCylinderBalanceDto>.Success(_mapper.Map<CustomerCylinderBalanceDto>(balance));
    }
}

public class CustomerCylinderBalanceDto : IMapFrom<CustomerCylinderBalance>
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Guid BrandId { get; set; }
    public string BrandName { get; set; } = string.Empty;
    public Guid CylinderSizeId { get; set; }
    public string CylinderSizeName { get; set; } = string.Empty;
    public int Received { get; set; }
    public int Returned { get; set; }
    public int Outstanding { get; set; }
}

public class AdjustCylinderBalanceRequest
{
    public Guid CustomerId { get; set; }
    public Guid BrandId { get; set; }
    public Guid CylinderSizeId { get; set; }
    public int Quantity { get; set; }
    public bool IsReturn { get; set; }
    /// <summary>Optional: credit sales order whose due is reduced by this return.</summary>
    public Guid? SettlementSalesOrderId { get; set; }
    /// <summary>Optional: amount credited against the order when cylinders are returned.</summary>
    public decimal SettlementAmount { get; set; }
}
