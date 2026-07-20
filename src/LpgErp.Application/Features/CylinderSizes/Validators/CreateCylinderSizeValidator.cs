using FluentValidation;
using LpgErp.Application.Features.CylinderSizes.DTOs;

namespace LpgErp.Application.Features.CylinderSizes.Validators;

public class CreateCylinderSizeValidator : AbstractValidator<CreateCylinderSizeRequest>
{
    public CreateCylinderSizeValidator()
    {
        RuleFor(x => x.BrandId)
            .NotEmpty().WithMessage("Brand is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Cylinder size name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

        RuleFor(x => x.WeightKg)
            .GreaterThan(0).WithMessage("Weight must be greater than 0.");

        RuleFor(x => x.DepositAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Deposit amount cannot be negative.");
    }
}
