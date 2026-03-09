using FluentValidation;

namespace ECommerce.ProductService.Features.CreateProduct;

public sealed class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.BasePrice).GreaterThan(0);
        RuleFor(x => x.CategoryId).NotEmpty();

        RuleForEach(x => x.Attributes).ChildRules(attr =>
        {
            attr.RuleFor(a => a.Key).NotEmpty().MaximumLength(100);
            attr.RuleFor(a => a.Value).NotEmpty().MaximumLength(500);
            attr.RuleFor(a => a.Unit).MaximumLength(50).When(a => a.Unit is not null);
        });

        RuleFor(x => x.Attributes)
            .Must(attrs => attrs.Select(a => a.Key).Distinct(StringComparer.OrdinalIgnoreCase).Count() == attrs.Count)
            .WithMessage("Attribute keys must be unique.")
            .When(x => x.Attributes.Count > 0);
    }
}
