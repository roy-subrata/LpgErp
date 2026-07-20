using FluentValidation;
using LpgErp.Application.Features.Drivers.DTOs;

namespace LpgErp.Application.Features.Drivers.Validators;

public class CreateDriverValidator : AbstractValidator<CreateDriverRequest>
{
    public CreateDriverValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Driver name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");
    }
}
