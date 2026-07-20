using FluentValidation;
using LpgErp.Application.Features.Payments.DTOs;

namespace LpgErp.Application.Features.Payments.Validators;

public class CreatePaymentValidator : AbstractValidator<CreatePaymentRequest>
{
    public CreatePaymentValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Payment amount must be greater than 0.");

        RuleFor(x => x.PaymentDate)
            .NotEmpty().WithMessage("Payment date is required.");

        RuleFor(x => x.SalesOrderId)
            .NotEmpty().When(x => x.PurchaseOrderId == null)
            .WithMessage("Either Sales Order or Purchase Order is required.");
    }
}
