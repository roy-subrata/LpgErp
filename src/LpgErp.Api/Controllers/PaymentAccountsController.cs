using LpgErp.Application.Features.PaymentAccounts;
using LpgErp.Application.Features.PaymentAccounts.DTOs;

namespace LpgErp.Api.Controllers;

public class PaymentAccountsController : BaseController<CreatePaymentAccountRequest, UpdatePaymentAccountRequest, PaymentAccountDto>
{
    public PaymentAccountsController(IPaymentAccountService service) : base(service) { }
}
