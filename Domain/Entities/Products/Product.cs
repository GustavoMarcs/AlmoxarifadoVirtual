using Domain.Entities.Suppliers;
using Domain.Entities.Tracker;

namespace Domain.Entities.Products;

public sealed class Product : EntityBase
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal SellingPrice { get; set; }

    public int Amount { get; set; }

    public int MinimalQuantity { get; set; }

    public int MaximalQuantity { get; set; }

    public ICollection<ProductPriceHistory> ProductPriceHistories { get; set; } = [];

    public ICollection<Movement> Movements { get; set; } = [];

    public long SupplierProductId { get; set; }
    public SupplierProduct SupplierProduct { get; set; } = null!;
    
    public long ProductLocationId { get; set; }
    public DepartmentLocation ProductLocation { get; set; } = null!;
}
