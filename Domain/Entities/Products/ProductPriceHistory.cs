using System.ComponentModel.DataAnnotations;

namespace Domain.Entities.Products;

public sealed class ProductPriceHistory
{
    [Key]
    public long Id { get; init; }
    
    public decimal OldPrice { get; init; }
    
    public decimal NewPrice { get; init; }

    public DateTime UpdatedPriceAt { get; init; }

    public long ProductId { get; init; }

    public Product Product { get; init; } = null!;
}