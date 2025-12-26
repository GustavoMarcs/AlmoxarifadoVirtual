using Domain.Entities.Suppliers;
using FluentValidation;

namespace Domain.Validations;

public class SupplierProductValidator : AbstractValidator<SupplierProduct>
{
    public SupplierProductValidator()
    {
        RuleFor(sp => sp.Name)
            .NotEmpty().WithMessage("O nome do produto é obrigatório")
            .MaximumLength(200).WithMessage("O nome do produto deve ter no máximo 200 caracteres");

        RuleFor(sp => sp.Price)
            .GreaterThan(0).WithMessage("O preço deve ser maior que zero");

        RuleFor(sp => sp.Barcode)
            .NotEmpty().WithMessage("O código de barras é obrigatório")
            .MaximumLength(50).WithMessage("O código de barras deve ter no máximo 50 caracteres")
            .Matches("^[0-9]+$").WithMessage("O código de barras deve conter apenas números");

        RuleFor(sp => sp.Description)
            .MaximumLength(1000).WithMessage("A descrição deve ter no máximo 1000 caracteres")
            .When(sp => !string.IsNullOrEmpty(sp.Description));

        RuleFor(sp => sp.SupplierId)
            .GreaterThan(0).WithMessage("O fornecedor é obrigatório");

        RuleFor(sp => sp.ProductCategoryId)
            .GreaterThan(0).WithMessage("A categoria é obrigatória");
    }
}
