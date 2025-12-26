using Domain.Entities;
using FluentValidation;

namespace Domain.Validations;

public class CountryValidator : AbstractValidator<Country>
{
    public CountryValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("O nome do país é obrigatório");

        RuleFor(c => c.Code)
            .NotEmpty().WithMessage("O código do país é obrigatório");
    }
}
