using FluentValidation;
using LpgErp.Application.Features.Settings.DTOs;

namespace LpgErp.Application.Features.Settings.Validators;

public class UpdateCompanySettingsValidator : AbstractValidator<UpdateCompanySettingsRequest>
{
    public UpdateCompanySettingsValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Company name is required.")
            .MaximumLength(200);

        RuleFor(x => x.Address).MaximumLength(500);
        RuleFor(x => x.Phone).MaximumLength(50);
        RuleFor(x => x.Email).MaximumLength(200).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Website).MaximumLength(200);
    }
}
