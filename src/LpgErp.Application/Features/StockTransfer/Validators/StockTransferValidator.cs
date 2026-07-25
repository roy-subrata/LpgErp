using FluentValidation;

namespace LpgErp.Application.Features.StockTransfer.Validators;

public class StockTransferValidator : AbstractValidator<StockTransferRequest>
{
    public StockTransferValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product is required.");

        RuleFor(x => x.FromWarehouseId)
            .NotEmpty().WithMessage("Source warehouse is required.");

        RuleFor(x => x.ToWarehouseId)
            .NotEmpty().WithMessage("Destination warehouse is required.")
            .NotEqual(x => x.FromWarehouseId).WithMessage("Source and destination warehouses must be different.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Transfer quantity must be greater than zero.");
    }
}
