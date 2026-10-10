using ECommerce.Application.Models;
using FluentValidation;

namespace ECommerce.Application.Services.Validators
{
    public class GetAllProductsOptionsValidator : AbstractValidator<GetAllProductsOptions>
    {
        public GetAllProductsOptionsValidator()
        {
            RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
            RuleFor(x => x.PageSize).InclusiveBetween(1, 50)
                .WithMessage("You can get between 1 and 50 products per page.");
            RuleFor(x => x.MinPrice).GreaterThanOrEqualTo(0).When(x => x.MinPrice.HasValue);
            RuleFor(x => x.MaxPrice).GreaterThanOrEqualTo(x => x.MinPrice!.Value)
                .When(x => x.MinPrice.HasValue && x.MaxPrice.HasValue)
                .WithMessage("MaxPrice must be greater than or equal to MinPrice.");
            RuleFor(x => x.Category).IsInEnum().When(x => x.Category.HasValue);
        }
    }
}
