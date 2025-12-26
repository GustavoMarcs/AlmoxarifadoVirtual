using Domain.Entities.Products;
using FluentValidation;

namespace Domain.Validations;

public class ProductCategoryValidator : AbstractValidator<ProductCategory>
{
    public ProductCategoryValidator()
    {
        RuleFor(pc => pc.Name)
            .NotEmpty().WithMessage("O nome da categoria é obrigatório")
            .MaximumLength(100).WithMessage("O nome da categoria deve ter no máximo 100 caracteres");
    }
}
