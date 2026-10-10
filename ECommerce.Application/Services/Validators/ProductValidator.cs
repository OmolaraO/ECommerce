using ECommerce.Application.Models;
using FluentValidation;

namespace ECommerce.Application.Services.Validators
{
    public class ProductValidator : AbstractValidator<Product>
    {
        public ProductValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Name).NotEmpty().MaximumLength(255);
            RuleFor(x => x.Description).NotNull().MaximumLength(1000);
            RuleFor(x => x.Price).GreaterThan(0).PrecisionScale(18, 2, ignoreTrailingZeros: true);
            RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Category).IsInEnum();
        }
    }
}
