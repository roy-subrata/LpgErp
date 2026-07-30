using AutoMapper;
using LpgErp.Application.Common.Interfaces;
using LpgErp.Application.Common.Models;
using LpgErp.Application.Features.PaymentAccounts.DTOs;
using LpgErp.Domain.Entities;
using LpgErp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LpgErp.Application.Features.PaymentAccounts;

public interface IPaymentAccountService : IService<CreatePaymentAccountRequest, UpdatePaymentAccountRequest, PaymentAccountDto> { }

public class PaymentAccountService : IPaymentAccountService
{
    private readonly IApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public PaymentAccountService(IApplicationDbContext context, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<PaymentAccountDto>>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.PaymentAccounts
            .Where(a => !a.IsDeleted)
            .OrderBy(a => a.Method)
            .ThenBy(a => a.Name);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return Result<PagedResult<PaymentAccountDto>>.Success(new PagedResult<PaymentAccountDto>
        {
            Items = _mapper.Map<IReadOnlyList<PaymentAccountDto>>(items),
            Pagination = new PaginationMeta { PageNumber = pageNumber, PageSize = pageSize, TotalCount = totalCount }
        });
    }

    /// <summary>Only active accounts — this feeds the pickers on the payment and order forms.</summary>
    public async Task<Result<IReadOnlyList<PaymentAccountDto>>> GetAllListAsync(CancellationToken cancellationToken = default)
    {
        var items = await _context.PaymentAccounts
            .Where(a => !a.IsDeleted && a.IsActive)
            .OrderBy(a => a.Method)
            .ThenBy(a => a.Name)
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<PaymentAccountDto>>.Success(_mapper.Map<IReadOnlyList<PaymentAccountDto>>(items));
    }

    public async Task<Result<PaymentAccountDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.PaymentAccounts.FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted, cancellationToken);
        if (entity is null) return Result<PaymentAccountDto>.Failure("Payment account not found.");

        return Result<PaymentAccountDto>.Success(_mapper.Map<PaymentAccountDto>(entity));
    }

    public async Task<Result<PaymentAccountDto>> CreateAsync(CreatePaymentAccountRequest createDto, CancellationToken cancellationToken = default)
    {
        if (await NameTakenAsync(createDto.Name, null, cancellationToken))
            return Result<PaymentAccountDto>.Failure($"A payment account named '{createDto.Name}' already exists.");

        var entity = _mapper.Map<PaymentAccount>(createDto);
        await _context.PaymentAccounts.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<PaymentAccountDto>.Success(_mapper.Map<PaymentAccountDto>(entity));
    }

    public async Task<Result<PaymentAccountDto>> UpdateAsync(Guid id, UpdatePaymentAccountRequest updateDto, CancellationToken cancellationToken = default)
    {
        var entity = await _context.PaymentAccounts.FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted, cancellationToken);
        if (entity is null) return Result<PaymentAccountDto>.Failure("Payment account not found.");

        if (await NameTakenAsync(updateDto.Name, id, cancellationToken))
            return Result<PaymentAccountDto>.Failure($"A payment account named '{updateDto.Name}' already exists.");

        // Changing the method would strand payments recorded under the old one — their method
        // and their account's method must agree for a per-channel breakdown to mean anything.
        if (entity.Method != updateDto.Method && await HasPaymentsAsync(id, cancellationToken))
            return Result<PaymentAccountDto>.Failure("This account already has payments recorded against it, so its method cannot be changed. Deactivate it and create a new account instead.");

        _mapper.Map(updateDto, entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<PaymentAccountDto>.Success(_mapper.Map<PaymentAccountDto>(entity));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.PaymentAccounts.FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted, cancellationToken);
        if (entity is null) return Result.Failure("Payment account not found.");

        if (await HasPaymentsAsync(id, cancellationToken))
            return Result.Failure("This account has payments recorded against it and cannot be deleted. Deactivate it instead so it stops appearing on new payments.");

        _context.PaymentAccounts.Remove(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private Task<bool> NameTakenAsync(string name, Guid? excludingId, CancellationToken cancellationToken) =>
        _context.PaymentAccounts.AnyAsync(
            a => !a.IsDeleted && a.Name == name && (excludingId == null || a.Id != excludingId),
            cancellationToken);

    private Task<bool> HasPaymentsAsync(Guid accountId, CancellationToken cancellationToken) =>
        _context.Payments.AnyAsync(p => !p.IsDeleted && p.PaymentAccountId == accountId, cancellationToken);
}
