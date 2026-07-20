using FluentValidation;
using LpgErp.Application.Features.Salesmen.DTOs;

namespace LpgErp.Application.Features.Salesmen.Validators;

public class CreateSalesmanValidator : AbstractValidator<CreateSalesmanRequest>
{
    public CreateSalesmanValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Salesman name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.DailyCommissionRate)
            .GreaterThanOrEqualTo(0).WithMessage("Commission rate cannot be negative.");

        RuleFor(x => x.DailyAllowance)
            .GreaterThanOrEqualTo(0).WithMessage("Daily allowance cannot be negative.");
    }
}
