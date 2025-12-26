using Domain.Entities;
using FluentValidation;

namespace Domain.Validations;

public class DepartmentLocationValidator : AbstractValidator<DepartmentLocation>
{
    public DepartmentLocationValidator()
    {
        RuleFor(d => d.Name)
            .NotEmpty().WithMessage("O nome da localização é obrigatório")
            .MaximumLength(200).WithMessage("O nome da localização deve ter no máximo 200 caracteres");

        RuleFor(d => d.Capacity)
            .GreaterThan(0).WithMessage("A capacidade deve ser maior ou igual a 1");

        RuleFor(d => d.Description)
            .MaximumLength(35).WithMessage("A quantidade máxima de caracteres é 35, seja breve")
            .When(d => !string.IsNullOrEmpty(d.Description));
    }
}
