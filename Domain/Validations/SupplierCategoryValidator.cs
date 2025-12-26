using Domain.Entities.Suppliers;
using FluentValidation;

namespace Domain.Validations;

public class SupplierCategoryValidator : AbstractValidator<SupplierCategory>
{
    public SupplierCategoryValidator()
    {
        RuleFor(sc => sc.Name)
            .NotEmpty().WithMessage("O nome da categoria é obrigatório")
            .MaximumLength(50).WithMessage("O nome da categoria deve ter no máximo 50 caracteres");

        RuleFor(sc => sc.Description)
            .MaximumLength(35).WithMessage("A descrição deve ter no máximo 35 caracteres, seja breve")
            .When(sc => !string.IsNullOrEmpty(sc.Description));
    }
}
