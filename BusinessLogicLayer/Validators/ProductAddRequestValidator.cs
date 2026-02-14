using BusinessLogicLayer.DTOs;
using FluentValidation;

namespace BusinessLogicLayer.Validators;

// 39
public class ProductAddRequestValidator : AbstractValidator<ProductAddRequest>
{
    public ProductAddRequestValidator()
    {
        RuleFor(x => x.ProductName).NotEmpty().WithMessage("Product Name is required.");
        RuleFor(x => x.Category).NotEmpty().WithMessage("Category is required.");
        RuleFor(x => x.UnitPrice).InclusiveBetween(0, double.MaxValue).WithMessage(string.Format("Unit Price should be between 0 " +
            "and {0}", double.MaxValue));
        RuleFor(x => x.QuantityInStock).InclusiveBetween(0, int.MaxValue).WithMessage(string.Format("Quantity in stock should be between 0 " +
            "and {0}", int.MaxValue));
    }
}
