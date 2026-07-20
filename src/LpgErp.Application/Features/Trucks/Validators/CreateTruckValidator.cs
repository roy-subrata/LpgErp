using FluentValidation;
using LpgErp.Application.Features.Trucks.DTOs;

namespace LpgErp.Application.Features.Trucks.Validators;

public class CreateTruckValidator : AbstractValidator<CreateTruckRequest>
{
    public CreateTruckValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Truck name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");
    }
}
