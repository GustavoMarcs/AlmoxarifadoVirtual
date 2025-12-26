using Domain.Entities.Tracker;
using Domain.Enums;
using Domain.Interfaces.Products;
using FluentValidation;

namespace Domain.Validations;

public class MovementValidator : AbstractValidator<Movement>
{
    public MovementValidator(IProductService productService)
    {
        var productService1 = productService;

        RuleFor(m => m.Quantity)
            .GreaterThan(0).WithMessage("A quantidade deve ser maior que zero")
            .LessThanOrEqualTo(10000).WithMessage("A quantidade deve ser menor ou igual a 10.000");

        RuleFor(m => m.Type)
            .IsInEnum().WithMessage("O tipo de movimentação deve ser válido");

        RuleFor(m => m.ProductId)
            .GreaterThan(0).WithMessage("O produto é obrigatório");

        RuleFor(m => m)
            .MustAsync(async (m, ct) =>
            {
                if (m.Type == MovementType.Out)
                {
                    var product = await productService1.GetByIdAsync(m.ProductId, ct);
                    return product is not null && m.Quantity <= product.Amount;
                }
                
                return true;
            })
            .WithMessage("A quantidade de saída não pode ser maior que o estoque atual do produto");
        
        RuleFor(m => m)
            .MustAsync(async (m, ct) =>
            {
                if (m.Type == MovementType.In)
                {
                    var product = await productService1.GetByIdAsync(m.ProductId, ct);
                    return product is not null && m.Quantity + product.Amount <= product.MaximalQuantity;
                }
                
                return true;
            })
            .WithMessage("A quantidade de entrada somada com o atual estoque do produto " +
                         "não pode ultrapassar a quantidade máxima de produtos");
    }
}
