using FluentValidation;
using LpgErp.Application.Features.Brands.DTOs;

namespace LpgErp.Application.Features.Brands.Validators;

public class CreateBrandValidator : AbstractValidator<CreateBrandRequest>
{
    public CreateBrandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Brand name is required.")
            .MaximumLength(200).WithMessage("Brand name must not exceed 200 characters.");
    }
}
