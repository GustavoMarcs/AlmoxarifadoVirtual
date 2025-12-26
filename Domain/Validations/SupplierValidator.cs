using Domain.Entities.Suppliers;
using FluentValidation;

namespace Domain.Validations;

public class SupplierValidator : AbstractValidator<Supplier>
{
    public SupplierValidator()
    {
        RuleFor(s => s.TradeName)
            .NotEmpty().WithMessage("O nome do fornecedor é obrigatório")
            .MaximumLength(200).WithMessage("O nome do fornecedor deve ter no máximo 200 caracteres");

        RuleFor(s => s.CorporateName)
            .NotEmpty().WithMessage("A razão social é obrigatória")
            .MaximumLength(200).WithMessage("A razão social deve ter no máximo 200 caracteres");

        RuleFor(s => s.Cnpj)
            .NotEmpty().WithMessage("O CNPJ é obrigatório")
            .MaximumLength(18).WithMessage("O CNPJ deve ter no máximo 18 caracteres")
            .Matches(@"^\d{2}\.\d{3}\.\d{3}/\d{4}-\d{2}$").WithMessage("O CNPJ deve estar no formato XX.XXX.XXX/XXXX-XX");

        RuleFor(s => s.Phone)
            .NotEmpty().WithMessage("O telefone é obrigatório")
            .MaximumLength(20).WithMessage("O telefone deve ter no máximo 20 caracteres")
            .Matches(@"^\+?[0-9\s\-\(\)]+$").WithMessage("O telefone deve conter apenas números, espaços, hífens e parênteses");

        RuleFor(s => s.Email)
            .NotEmpty().WithMessage("O email é obrigatório")
            .MaximumLength(100).WithMessage("O email deve ter no máximo 100 caracteres")
            .EmailAddress().WithMessage("O email deve estar em um formato válido");

        RuleFor(s => s.Address).SetValidator(new AddressValidator());

        RuleFor(s => s.SupplierCategoryId)
            .GreaterThan(0).WithMessage("A categoria do fornecedor é obrigatória");
    }
}
