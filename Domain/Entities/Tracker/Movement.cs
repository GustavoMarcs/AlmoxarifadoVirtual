using Domain.Entities.Products;
using Domain.Enums;

namespace Domain.Entities.Tracker;

public class Movement : EntityBase
{
    public int Quantity { get; set; }
    
    public MovementType Type { get; set; }

    public long ProductId { get; set; }
    public Product Product { get; set; } = null!;
}