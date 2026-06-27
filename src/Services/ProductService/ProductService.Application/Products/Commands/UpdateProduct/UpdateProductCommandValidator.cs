using FluentValidation;

namespace ProductService.Application.Products.Commands.UpdateProduct;

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(v => v.Id)
            .GreaterThan(0).WithMessage("Product ID must be greater than zero.");

        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(100).WithMessage("Product name must not exceed 100 characters.");

        RuleFor(v => v.Price)
            .GreaterThan(0).WithMessage("Price must be greater than zero.");

        RuleFor(v => v.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("Stock must be a non-negative integer.");
    }
}
