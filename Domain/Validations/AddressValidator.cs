using Domain.Entities;
using FluentValidation;

namespace Domain.Validations;

public class AddressValidator : AbstractValidator<Address>
{
    public AddressValidator()
    {
        RuleFor(a => a.StreetAdress)
            .NotEmpty().WithMessage("A rua é obrigatória")
            .MaximumLength(150).WithMessage("A rua deve ter no máximo 150 caracteres");

        RuleFor(a => a.City)
            .NotEmpty().WithMessage("A cidade é obrigatória")
            .MaximumLength(100).WithMessage("A cidade deve ter no máximo 100 caracteres");

        RuleFor(a => a.State)
            .NotEmpty().WithMessage("O estado é obrigatório")
            .MaximumLength(100).WithMessage("O estado deve ter no máximo 100 caracteres");

        RuleFor(a => a.ZipCode)
            .NotEmpty().WithMessage("O código postal é obrigatório")
            .Length(3, 12).WithMessage("O código postal deve ter entre 3 e 12 caracteres")
            .Matches(@"^[A-Za-z0-9\- ]+$").WithMessage("O código postal deve conter apenas letras, números, espaços ou hífens");

        RuleFor(a => a.CountryId).NotNull()
            .GreaterThan(0).WithMessage("O país selecionado é inválido");
    }
}
