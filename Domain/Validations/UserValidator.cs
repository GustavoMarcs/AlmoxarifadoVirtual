using Domain.Entities.Users;
using FluentValidation;

namespace Domain.Validations;

public class UserValidator : AbstractValidator<User>
{
    public UserValidator()
    {
        RuleFor(u => u.Name)
            .NotEmpty().WithMessage("O nome do usuário é obrigatório")
            .MaximumLength(100).WithMessage("O nome do usuário deve ter no máximo 100 caracteres");

        RuleFor(u => u.Email)
            .NotEmpty().WithMessage("O email é obrigatório")
            .MaximumLength(100).WithMessage("O email deve ter no máximo 100 caracteres")
            .EmailAddress().WithMessage("O email deve estar em um formato válido");

        RuleFor(u => u.PasswordHash)
            .NotEmpty().WithMessage("A senha é obrigatória")
            .MaximumLength(255).WithMessage("A senha deve ter no máximo 255 caracteres");

        RuleFor(u => u.Role)
            .NotEmpty().WithMessage("O papel do usuário é obrigatório")
            .MaximumLength(50).WithMessage("O papel do usuário deve ter no máximo 50 caracteres")
            .Matches("^(Admin|Manager)$").WithMessage("O papel deve ser Admin ou Manager");
    }
}
