using LpgErp.Application.Common.Mappings;
using LpgErp.Domain.Entities;

namespace LpgErp.Application.Features.PaymentAccounts.DTOs;

public class PaymentAccountDto : IMapFrom<PaymentAccount>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public PaymentMethod Method { get; set; }
    public string? AccountNumber { get; set; }
    public string? Provider { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
}

public class CreatePaymentAccountRequest : IMapTo<PaymentAccount>
{
    public string Name { get; set; } = string.Empty;
    public PaymentMethod Method { get; set; }
    public string? AccountNumber { get; set; }
    public string? Provider { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
}

public class UpdatePaymentAccountRequest : IMapTo<PaymentAccount>
{
    public string Name { get; set; } = string.Empty;
    public PaymentMethod Method { get; set; }
    public string? AccountNumber { get; set; }
    public string? Provider { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
}
