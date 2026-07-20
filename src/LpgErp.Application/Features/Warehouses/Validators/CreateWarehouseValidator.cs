using FluentValidation;
using LpgErp.Application.Features.Warehouses.DTOs;

namespace LpgErp.Application.Features.Warehouses.Validators;

public class CreateWarehouseValidator : AbstractValidator<CreateWarehouseRequest>
{
    public CreateWarehouseValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Warehouse name is required.")
            .MaximumLength(200).WithMessage("Warehouse name must not exceed 200 characters.");
    }
}
