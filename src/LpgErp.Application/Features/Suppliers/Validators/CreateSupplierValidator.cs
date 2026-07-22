using FluentValidation;
using LpgErp.Application.Features.Suppliers.DTOs;

namespace LpgErp.Application.Features.Suppliers.Validators;

public class CreateSupplierValidator : AbstractValidator<CreateSupplierRequest>
{
    public CreateSupplierValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Supplier name is required.")
            .MaximumLength(200).WithMessage("Supplier name must not exceed 200 characters.");

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("Invalid email address.");

        RuleFor(x => x.CommissionPerCylinder)
            .GreaterThanOrEqualTo(0).WithMessage("Commission per cylinder cannot be negative.");
    }
}
