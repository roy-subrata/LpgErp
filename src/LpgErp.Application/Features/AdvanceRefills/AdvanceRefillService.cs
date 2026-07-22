using AutoMapper;
using LpgErp.Application.Common.Interfaces;
using LpgErp.Application.Common.Models;
using LpgErp.Domain.Interfaces;
using LpgErp.Application.Features.AdvanceRefills.DTOs;
using LpgErp.Application.Features.SalesOrders;
using LpgErp.Application.Features.SalesOrders.DTOs;
using LpgErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LpgErp.Application.Features.AdvanceRefills;

public interface IAdvanceRefillService
{
    Task<Result<AdvanceRefillDto>> CreateAsync(CreateAdvanceRefillRequest request, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<OutstandingCylinderDto>>> GetOutstandingAsync(CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<AdvanceRefillDto>>> GetByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
}

public class AdvanceRefillService : IAdvanceRefillService
{
    private readonly IApplicationDbContext _context;
    private readonly ISalesOrderService _salesOrderService;
    private readonly IMapper _mapper;

    public AdvanceRefillService(IApplicationDbContext context, ISalesOrderService salesOrderService, IMapper mapper)
    {
        _context = context;
        _salesOrderService = salesOrderService;
        _mapper = mapper;
    }

    /// <summary>
    /// An advance refill is a normal credit refill sale with zero empties returned — the customer
    /// takes the filled cylinder now and owes both the money and the empty. Delegating to the sales
    /// flow gives stock deduction, movement records, and the cylinder ledger in one consistent path.
    /// </summary>
    public async Task<Result<AdvanceRefillDto>> CreateAsync(CreateAdvanceRefillRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Quantity <= 0)
            return Result<AdvanceRefillDto>.Failure("Quantity must be positive.");

        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == request.CustomerId && !c.IsDeleted, cancellationToken);
        if (customer is null)
            return Result<AdvanceRefillDto>.Failure("Customer not found.");

        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == request.ProductId && !p.IsDeleted, cancellationToken);
        if (product is null)
            return Result<AdvanceRefillDto>.Failure("Product not found.");
        if (product.Type != ProductType.GasRefill)
            return Result<AdvanceRefillDto>.Failure("Advance refills apply to gas-refill products only.");
        if (product.BrandId is null || product.CylinderSizeId is null)
            return Result<AdvanceRefillDto>.Failure("The refill product must have a brand and cylinder size for cylinder tracking.");

        var created = await _salesOrderService.CreateAsync(new CreateSalesOrderRequest
        {
            CustomerId = request.CustomerId,
            WarehouseId = request.WarehouseId,
            IsCreditSale = true,
            DueDate = DateTime.UtcNow.AddDays(customer.PaymentDueDays),
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? "Advance refill" : $"Advance refill — {request.Notes}",
            Items =
            [
                new CreateSalesOrderItemRequest
                {
                    ProductId = request.ProductId,
                    Quantity = request.Quantity,
                    UnitPrice = product.SalePrice,
                    EmptyReturnedQuantity = 0 // no empty handed over — the whole point of an advance refill
                }
            ]
        }, cancellationToken);
        if (!created.IsSuccess) return Result<AdvanceRefillDto>.Failure(created.Error!);

        var confirmed = await _salesOrderService.ConfirmAsync(created.Data!.Id, cancellationToken);
        if (!confirmed.IsSuccess) return Result<AdvanceRefillDto>.Failure(confirmed.Error!);

        var delivered = await _salesOrderService.DeliverAsync(created.Data.Id, cancellationToken);
        if (!delivered.IsSuccess)
            return Result<AdvanceRefillDto>.Failure(
                $"Order {created.Data.OrderNumber} was created but could not be delivered: {delivered.Error}");

        var balance = await _context.CustomerCylinderBalances
            .Include(b => b.Customer).Include(b => b.Brand).Include(b => b.CylinderSize)
            .FirstOrDefaultAsync(b => b.CustomerId == request.CustomerId
                && b.BrandId == product.BrandId
                && b.CylinderSizeId == product.CylinderSizeId
                && !b.IsDeleted, cancellationToken);

        return Result<AdvanceRefillDto>.Success(_mapper.Map<AdvanceRefillDto>(balance));
    }

    public async Task<Result<IReadOnlyList<OutstandingCylinderDto>>> GetOutstandingAsync(CancellationToken cancellationToken = default)
    {
        var balances = await _context.CustomerCylinderBalances
            .Include(b => b.Customer)
            .Include(b => b.Brand)
            .Include(b => b.CylinderSize)
            .Where(b => (b.Received - b.Returned) > 0)
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<OutstandingCylinderDto>>.Success(
            _mapper.Map<IReadOnlyList<OutstandingCylinderDto>>(balances));
    }

    public async Task<Result<IReadOnlyList<AdvanceRefillDto>>> GetByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var balances = await _context.CustomerCylinderBalances
            .Include(b => b.Customer)
            .Include(b => b.Brand)
            .Include(b => b.CylinderSize)
            .Where(b => b.CustomerId == customerId && (b.Received - b.Returned) > 0)
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<AdvanceRefillDto>>.Success(
            _mapper.Map<IReadOnlyList<AdvanceRefillDto>>(balances));
    }
}
