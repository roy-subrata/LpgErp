using FluentValidation;
using LpgErp.Application.Features.Cylinders.DTOs;

namespace LpgErp.Application.Features.Cylinders.Validators;

public class CreateCylinderValidator : AbstractValidator<CreateCylinderRequest>
{
    public CreateCylinderValidator()
    {
        RuleFor(x => x.BrandId)
            .NotEmpty().WithMessage("Brand is required.");

        RuleFor(x => x.CylinderSizeId)
            .NotEmpty().WithMessage("Cylinder size is required.");

        RuleFor(x => x.SerialNumber)
            .NotEmpty().WithMessage("Serial number is required.")
            .MaximumLength(100).WithMessage("Serial number must not exceed 100 characters.");
    }
}
