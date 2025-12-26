using Domain.Enums;

namespace Domain.Filters;

public class MovementRegister
{
    public long ProductId { get; set; }
    public MovementType Type { get; set; }
    public int Quantity { get; set; }

    public MovementRegister()
    {
    }

    public MovementRegister(long productId, MovementType type, int quantity)
    {
        ProductId = productId;
        Type = type;
        Quantity = quantity;
    }
}