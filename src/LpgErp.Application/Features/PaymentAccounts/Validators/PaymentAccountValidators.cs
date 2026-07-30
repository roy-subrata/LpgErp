using FluentValidation;
using LpgErp.Application.Features.PaymentAccounts.DTOs;
using LpgErp.Domain.Entities;

namespace LpgErp.Application.Features.PaymentAccounts.Validators;

public class CreatePaymentAccountValidator : AbstractValidator<CreatePaymentAccountRequest>
{
    public CreatePaymentAccountValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Account name is required.")
            .MaximumLength(200).WithMessage("Account name must not exceed 200 characters.");

        RuleFor(x => x.Method)
            .IsInEnum().WithMessage("Select a valid payment method.")
            .NotEqual(PaymentMethod.Credit).WithMessage("Credit is a sale term, not a payment channel. Use Cash, Bank, Mobile Banking, or Cheque.");

        RuleFor(x => x.AccountNumber)
            .NotEmpty().WithMessage("A wallet or account number is required for this method.")
            .When(x => x.Method is PaymentMethod.MobileBanking or PaymentMethod.Bank);

        RuleFor(x => x.AccountNumber).MaximumLength(50);
        RuleFor(x => x.Provider).MaximumLength(100);
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}

public class UpdatePaymentAccountValidator : AbstractValidator<UpdatePaymentAccountRequest>
{
    public UpdatePaymentAccountValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Account name is required.")
            .MaximumLength(200).WithMessage("Account name must not exceed 200 characters.");

        RuleFor(x => x.Method)
            .IsInEnum().WithMessage("Select a valid payment method.")
            .NotEqual(PaymentMethod.Credit).WithMessage("Credit is a sale term, not a payment channel. Use Cash, Bank, Mobile Banking, or Cheque.");

        RuleFor(x => x.AccountNumber)
            .NotEmpty().WithMessage("A wallet or account number is required for this method.")
            .When(x => x.Method is PaymentMethod.MobileBanking or PaymentMethod.Bank);

        RuleFor(x => x.AccountNumber).MaximumLength(50);
        RuleFor(x => x.Provider).MaximumLength(100);
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}
