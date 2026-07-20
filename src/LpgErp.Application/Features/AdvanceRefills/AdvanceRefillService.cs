using AutoMapper;
using LpgErp.Application.Common.Interfaces;
using LpgErp.Application.Common.Models;
using LpgErp.Domain.Interfaces;
using LpgErp.Application.Features.AdvanceRefills.DTOs;
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
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AdvanceRefillService(IApplicationDbContext context, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<AdvanceRefillDto>> CreateAsync(CreateAdvanceRefillRequest request, CancellationToken cancellationToken = default)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == request.CustomerId && !c.IsDeleted, cancellationToken);

        if (customer is null)
            return Result<AdvanceRefillDto>.Failure("Customer not found.");

        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == request.ProductId && !p.IsDeleted, cancellationToken);

        if (product is null)
            return Result<AdvanceRefillDto>.Failure("Product not found.");

        var order = new SalesOrder
        {
            OrderNumber = $"AR-{DateTime.UtcNow:yyyyMMddHHmmss}",
            CustomerId = request.CustomerId,
            WarehouseId = request.WarehouseId,
            Status = SalesOrderStatus.Confirmed,
            IsCreditSale = true,
            OrderDate = DateTime.UtcNow,
            Notes = request.Notes,
            Items = new List<SalesOrderItem>
            {
                new()
                {
                    ProductId = request.ProductId,
                    Quantity = request.Quantity,
                    UnitPrice = product.SalePrice
                }
            }
        };

        order.TotalAmount = order.Items.Sum(i => i.TotalPrice);

        await _context.SalesOrders.AddAsync(order, cancellationToken);

        var balance = await _context.CustomerCylinderBalances
            .FirstOrDefaultAsync(b =>
                b.CustomerId == request.CustomerId &&
                b.BrandId == product.BrandId &&
                b.CylinderSizeId == product.CylinderSizeId, cancellationToken);

        if (balance is null)
        {
            balance = new CustomerCylinderBalance
            {
                CustomerId = request.CustomerId,
                BrandId = product.BrandId!.Value,
                CylinderSizeId = product.CylinderSizeId!.Value,
                Received = request.Quantity,
                Returned = 0
            };
            await _context.CustomerCylinderBalances.AddAsync(balance, cancellationToken);
        }
        else
        {
            balance.Received += request.Quantity;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<AdvanceRefillDto>.Success(_mapper.Map<AdvanceRefillDto>(balance));
    }

    public async Task<Result<IReadOnlyList<OutstandingCylinderDto>>> GetOutstandingAsync(CancellationToken cancellationToken = default)
    {
        var balances = await _context.CustomerCylinderBalances
            .Include(b => b.Customer)
            .Include(b => b.Brand)
            .Include(b => b.CylinderSize)
            .Where(b => b.Outstanding > 0)
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
            .Where(b => b.CustomerId == customerId && b.Outstanding > 0)
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<AdvanceRefillDto>>.Success(
            _mapper.Map<IReadOnlyList<AdvanceRefillDto>>(balances));
    }
}
