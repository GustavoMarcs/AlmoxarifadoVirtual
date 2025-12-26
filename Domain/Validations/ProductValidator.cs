using Domain.Entities.Products;
using FluentValidation;

namespace Domain.Validations;

public class ProductValidator : AbstractValidator<Product>
{
    public ProductValidator()
    {
        RuleFor(p => p.Name)
            .NotEmpty().WithMessage("O nome do produto é obrigatório")
            .MaximumLength(200).WithMessage("O nome do produto deve ter no máximo 200 caracteres");

        RuleFor(p => p.SellingPrice)
            .GreaterThan(0).WithMessage("O preço de venda deve ser maior que zero");

        RuleFor(p => p.Amount)
            .GreaterThanOrEqualTo(0).WithMessage("A quantidade deve ser maior ou igual a zero");

        RuleFor(p => p.Amount)
            .LessThanOrEqualTo(p => p.MaximalQuantity)
            .When(p => p.Amount > p.MaximalQuantity && p.MaximalQuantity > 0)
            .WithMessage("A quantidade deve ser menor ou igual à quantidade máxima");

        RuleFor(p => p.MinimalQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("A quantidade mínima deve ser maior ou igual a zero");

        RuleFor(p => p.MinimalQuantity)
            .LessThanOrEqualTo(p => p.MaximalQuantity)
            .When(p => p.MaximalQuantity != 0)
            .WithMessage("A quantidade mínima deve ser menor ou igual à máxima");



        RuleFor(p => p.MaximalQuantity)
            .GreaterThan(0).WithMessage("A quantidade máxima deve ser maior ou igual a zero");

        RuleFor(p => p.SupplierProductId)
            .NotNull()
            .GreaterThan(0)
            .WithMessage("O produto do fornecedor é obrigatório");

        RuleFor(p => p.ProductLocationId)
            .NotNull()
            .GreaterThan(0)
            .WithMessage("A localização é obrigatória");
    }
}
